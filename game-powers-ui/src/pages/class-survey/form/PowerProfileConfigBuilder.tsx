import { useEffect, useMemo, useRef, useState } from 'react';
import classNames from 'classnames';
import jp from 'jsonpath';
import parseJsonAst, { Token } from 'json-to-ast';
import { RefreshIcon } from '@heroicons/react/solid';

import { useObservable } from 'core/hooks/useObservable';
import { is200 } from 'core/is200';
import { Button } from 'components/button/Button';
import { ButtonRow } from 'components/ButtonRow';
import { YamlEditor } from 'components/monaco/YamlEditor';
import { PowerTextBlock } from 'components/power';
import { PowerProfileConfig } from 'api/models/PowerProfileConfig';
import { AstViewer } from 'components/json/ast';
import { of, Subject } from 'rxjs';
import { useApi } from 'core/hooks/useApi';
import { map, filter, switchAll, startWith } from 'rxjs/operators';
import produce from 'immer';
import useConstant from 'use-constant';
import { powerProfileConfigSchema } from 'core/schemas/api';
import { powerTextBlockToProps } from 'components/power/PowerTextBlock';
import { SamplePowerData, SamplePowerRequestBody } from './SamplePowers';

const safePaths: typeof jp.paths = (obj, pathExpression, count) => {
	try {
		return jp.paths(obj, pathExpression, count);
	} catch (ex) {
		return [];
	}
};

export type PowerProfileConfigBuilderProps = SamplePowerRequestBody & {
	selectedPower?: SamplePowerData | null;
	onCancel?: () => void;
	onSave?: (powerProfileConfig: PowerProfileConfig) => void;
};

export function PowerProfileConfigBuilder({
	classProfile,
	toolIndex,
	powerProfileIndex,
	level,
	usage,
	selectedPower,
	onCancel,
	onSave,
}: PowerProfileConfigBuilderProps) {
	const powerProfileConfig =
		typeof powerProfileIndex === 'number' ? classProfile.tools[toolIndex].powerProfileConfigs[powerProfileIndex] : null;

	const api = useApi();
	const [updated, setUpdated] = useState(powerProfileConfig!);
	const [tab, setTab] = useState<'power' | 'tree' | 'json'>('power');
	const [useOriginal, setUseOriginal] = useState(true);
	const refreshOverride = useConstant(() => new Subject<void>());

	const currentPowerProfileConfig = useRef(updated);
	currentPowerProfileConfig.current = updated;

	const override = useObservable(
		(input$) =>
			input$.pipe(
				map(([shouldUseOriginal]: readonly [boolean]) =>
					shouldUseOriginal
						? of(null)
						: refreshOverride.pipe(
								startWith(null),
								map(() =>
									api.generateSamplePower({
										body: {
											classProfile:
												typeof powerProfileIndex === 'number'
													? produce(classProfile, (cp) => {
															// eslint-disable-next-line no-param-reassign
															cp.tools[toolIndex].powerProfileConfigs[powerProfileIndex] =
																currentPowerProfileConfig.current;
													  })
													: classProfile,
											toolIndex,
											powerProfileIndex,
											level,
											usage,
										},
									})
								),
								switchAll(),
								filter(is200),
								map((response) => response.data)
						  )
				),
				switchAll()
			),
		null as null | SamplePowerData,
		[useOriginal] as const
	);

	useEffect(() => {
		if (powerProfileConfig) setUpdated(powerProfileConfig);
	}, [powerProfileConfig]);

	function setUpdatedIfValid(value: PowerProfileConfig) {
		if (powerProfileConfigSchema.isValidSync(value)) setUpdated(value);
	}

	const power = override || selectedPower;

	const { ast, paths } = useMemo((): {
		ast: Token | null;
		paths: jp.PathComponent[][];
	} => {
		if (!power) return { ast: null, paths: [] };
		const jsonAst = parseJsonAst(power.powerJson);
		const parsed = JSON.parse(power.powerJson);
		const powerPaths = updated.powerChances.flatMap((powerChance) => safePaths(parsed, powerChance.selector));

		return {
			ast: jsonAst,
			paths: powerPaths,
		};
	}, [updated, power]);

	return (
		<div className="mt-2 grid grid-cols-3 gap-2">
			<div className="col-span-2">
				{updated && <YamlEditor value={updated} onChange={setUpdatedIfValid} path="power-profile-config.yaml" />}
			</div>
			<div
				className="flex flex-col gap-1"
				style={{ maxHeight: '60vh' /* TODO - this isn't the right spot to set this */ }}>
				<div className="flex justify-between">
					<label>
						<input type="checkbox" checked={useOriginal} onChange={(ev) => setUseOriginal(ev.currentTarget.checked)} />{' '}
						Use Original
					</label>
					<Button
						contents="icon"
						look="primary"
						className={classNames({ hidden: useOriginal })}
						onClick={() => refreshOverride.next()}>
						<RefreshIcon className="h-em w-em" />
					</Button>
				</div>
				<div className="flex p-1 gap-1 bg-blue-900 mb-2 rounded-xl">
					<button
						type="button"
						className={classNames(
							'w-full py-2.5 text-sm leading-5 font-medium text-blue-700 rounded-lg',
							'focus:outline-none focus:ring-2 ring-offset-2 ring-offset-blue-400 ring-white ring-opacity-60',
							tab === 'power' ? 'bg-white shadow' : 'text-blue-100 hover:bg-white/[0.12] hover:text-white'
						)}
						onClick={() => setTab('power')}>
						Power
					</button>
					<button
						type="button"
						className={classNames(
							'w-full py-2.5 text-sm leading-5 font-medium text-blue-700 rounded-lg',
							'focus:outline-none focus:ring-2 ring-offset-2 ring-offset-blue-400 ring-white ring-opacity-60',
							tab === 'tree' ? 'bg-white shadow' : 'text-blue-100 hover:bg-white/[0.12] hover:text-white'
						)}
						onClick={() => setTab('tree')}>
						Tree
					</button>
					<button
						type="button"
						className={classNames(
							'w-full py-2.5 text-sm leading-5 font-medium text-blue-700 rounded-lg',
							'focus:outline-none focus:ring-2 ring-offset-2 ring-offset-blue-400 ring-white ring-opacity-60',
							tab === 'json' ? 'bg-white shadow' : 'text-blue-100 hover:bg-white/[0.12] hover:text-white'
						)}
						onClick={() => setTab('json')}>
						JSON
					</button>
				</div>
				<div className="flex-1 overflow-y-auto">
					{power && tab === 'power' && <PowerTextBlock {...powerTextBlockToProps(power.power)} />}
					{power && tab === 'tree' && ast && (
						<>
							<AstViewer data={ast} highlight={paths} />
						</>
					)}
					{power && tab === 'json' && <pre className="text-xs">{power.powerJson}</pre>}
				</div>
			</div>

			<ul className="col-span-3 list-disc pl-6">
				<li>At least one Power Chance selector must match for the power.</li>
				<li>At least one Modifier Chance must match every modifier.</li>
				<li>
					The weights of all selectors that match are multiplied and used to determine how many chances the power has of
					being selected.
				</li>
			</ul>

			{(onSave || onCancel) && (
				<ButtonRow className="col-span-3">
					{onSave && updated && <Button onClick={() => onSave(updated)}>Save</Button>}
					{onCancel && (
						<Button look="cancel" onClick={onCancel}>
							Cancel
						</Button>
					)}
				</ButtonRow>
			)}
		</div>
	);
}
