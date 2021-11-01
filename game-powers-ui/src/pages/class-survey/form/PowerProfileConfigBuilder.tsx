import { useEffect, useMemo, useState } from 'react';
import classNames from 'classnames';
import jp from 'jsonpath';
import parseJsonAst, { Token } from 'json-to-ast';

import { Button } from 'components/button/Button';
import { ButtonRow } from 'components/ButtonRow';

import { YamlEditor } from 'components/monaco/YamlEditor';
import { PowerTextBlock } from 'components/power';
import { PowerType } from 'components/power/Power';
import { PowerProfileConfig } from 'api/models/PowerProfileConfig';
import { AstViewer } from 'components/json/ast';
import { SamplePowerData } from './SamplePowers';

export type PowerProfileConfigBuilderProps = {
	powerProfileConfig?: PowerProfileConfig | null;
	selectedPower?: SamplePowerData | null;
	onCancel?: () => void;
	onSave?: (powerProfileConfig: PowerProfileConfig) => void;
};

export function PowerProfileConfigBuilder({
	powerProfileConfig,
	selectedPower,
	onCancel,
	onSave,
}: PowerProfileConfigBuilderProps) {
	const [updated, setUpdated] = useState(powerProfileConfig!);
	const [tab, setTab] = useState<'power' | 'json'>('power');

	useEffect(() => {
		if (powerProfileConfig) setUpdated(powerProfileConfig);
	}, [powerProfileConfig]);

	const { ast, paths } = useMemo((): {
		ast: Token | null;
		paths: jp.PathComponent[][];
	} => {
		if (!selectedPower) return { ast: null, paths: [] };
		const jsonAst = parseJsonAst(selectedPower.powerJson);
		const parsed = JSON.parse(selectedPower.powerJson);

		const allModifiers = jp.paths(parsed, '$..Modifiers');
		const allModifierPaths = allModifiers.flatMap((path) =>
			path
				.slice(1)
				.reduce((prev: any, next) => prev[next], parsed)
				.flatMap((target: any, i: number) =>
					updated.modifierChances
						.flatMap((modChance) => jp.paths(target, modChance.selector))
						.map((modPath) => [...path, i, ...modPath.slice(1)])
				)
		);

		const powerPaths = updated.powerChances.flatMap((powerChance) => jp.paths(parsed, powerChance.selector));

		return {
			ast: jsonAst,
			paths: [powerPaths, ...allModifierPaths],
		};
	}, [updated, selectedPower]);

	return (
		<div className="mt-2 grid grid-cols-3 gap-2">
			<div className="col-span-2">
				{updated && <YamlEditor value={updated} onChange={setUpdated} path="power-profile-config.yaml" />}
			</div>
			<div>
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
							tab === 'json' ? 'bg-white shadow' : 'text-blue-100 hover:bg-white/[0.12] hover:text-white'
						)}
						onClick={() => setTab('json')}>
						JSON
					</button>
				</div>
				{selectedPower && tab === 'power' && (
					<PowerTextBlock
						{...selectedPower.power}
						powerUsage={selectedPower.power.powerUsage as PowerType}
						attackType={
							(selectedPower.power.attackType || null) as 'Personal' | 'Ranged' | 'Melee' | 'Close' | 'Area' | null
						}
					/>
				)}
				{selectedPower && tab === 'json' && ast && (
					<>
						<AstViewer data={ast} highlight={paths} />
					</>
				)}
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
