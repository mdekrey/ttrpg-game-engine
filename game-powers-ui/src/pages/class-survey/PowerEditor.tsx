import { ClassDetailsReadOnly } from 'api/models/ClassDetailsReadOnly';
import { PowerTextProfile } from 'api/models/PowerTextProfile';
import classNames from 'classnames';
import { Button } from 'components/button/Button';
import { TextboxField } from 'components/forms';
import { PowerTextBlock } from 'components/power';
import { PowerType } from 'components/power/Power';
import { useApi } from 'core/hooks/useApi';
import { useGameForm } from 'core/hooks/useGameForm';
import { useObservable } from 'core/hooks/useObservable';
import { initial, isLoading, Loadable, makeError, makeLoaded, makeLoading } from 'core/loadable/loadable';
import { LoadableComponent } from 'core/loadable/LoadableComponent';
import { Dictionary } from 'lodash';
import { groupBy } from 'lodash/fp';
import { delay, map, repeatWhen, startWith, switchAll, takeWhile } from 'rxjs/operators';
import * as yup from 'yup';
import { SaveIcon } from '@heroicons/react/solid';
import { ButtonRow } from 'components/ButtonRow';
import { Spinner } from 'components/spinner/spinner';
import { useMemo } from 'react';
import { Subject } from 'rxjs';
import { RequestParams } from 'api/operations/setPowerFlavor';

type PowerTextInfo = {
	name: string;
	flavorText: string;
};

const powerTextInfoSchema: yup.SchemaOf<PowerTextInfo> = yup.object({
	name: yup.string().required().label('Name'),
	flavorText: yup.string().required().label('Flavor Text'),
});

function PowerEdit({ power, param }: { power: PowerTextProfile; param: RequestParams }) {
	const api = useApi();
	const submitSubject = useMemo(() => new Subject<PowerTextInfo>(), []);
	const { powerUsage, attackType, name: originalName, flavorText: originalFlavor, ...text } = power.text;

	const { handleSubmit, ...form } = useGameForm<PowerTextInfo>({
		defaultValues: {
			name: originalName,
			flavorText: originalFlavor || '',
		},
		schema: powerTextInfoSchema,
	});

	const maybe = useObservable<Loadable<boolean>>(
		() =>
			submitSubject.pipe(
				map((data) =>
					api.setPowerFlavor(param, data, 'application/json').pipe(
						map((response) => (response.statusCode === 200 ? makeLoaded(true) : makeError(response))),
						startWith(makeLoading())
					)
				),
				switchAll()
			),
		initial
	);

	const disabled = isLoading(maybe);

	return (
		<div className="grid grid-cols-3 gap-4">
			<PowerTextBlock
				className="col-span-2"
				name={form.watch('name')}
				flavorText={form.watch('flavorText')}
				{...text}
				powerUsage={powerUsage as PowerType}
				attackType={(attackType || null) as 'Personal' | 'Ranged' | 'Melee' | 'Close' | 'Area' | null}
			/>
			<form
				onSubmit={disabled ? () => {} : handleSubmit((d) => submitSubject.next(d))}
				className="grid grid-cols-1 gap-3 self-start">
				<TextboxField label="Name" form={form} name="name" disabled={disabled} />
				<TextboxField label="Flavor Text" form={form} name="flavorText" disabled={disabled} />
				<ButtonRow>
					<Button contents="icon" type="submit" disabled={disabled}>
						{disabled ? <Spinner /> : <SaveIcon className="w-5 h-5" />}
					</Button>
				</ButtonRow>
			</form>
		</div>
	);
}

function PowerSection({
	header,
	powers,
	classId,
}: {
	header: string;
	powers: (PowerTextProfile & { index: number })[];
	classId: string;
}) {
	return (
		<section>
			{powers.map((p, i) => {
				return (
					// eslint-disable-next-line react/no-array-index-key
					<article className="mb-4" style={{ pageBreakInside: 'avoid' }} key={i}>
						{i === 0 && (
							<h2 className={classNames('font-header font-bold', 'mt-4 first:mt-0', 'text-theme text-xl')}>{header}</h2>
						)}
						<PowerEdit power={p} param={{ id: classId, _index: p.index }} />
					</article>
				);
			})}
		</section>
	);
}

type ReasonCode = 'NotFound';

export function PowerEditor({ data: { classId } }: { data: { classId: string } }) {
	const api = useApi();
	const powers = useObservable(
		(input$) =>
			input$.pipe(
				map(([id]) =>
					api.getClass({ id }).pipe(
						repeatWhen((completed) => completed.pipe(delay(1000))),
						takeWhile((response) => response.statusCode === 200 && response.data.inProgress === true, true),
						map((response) =>
							response.statusCode === 404
								? makeError<ReasonCode>('NotFound' as const)
								: response.data.inProgress
								? makeLoading(toPowerTextGroups(response.data.original))
								: makeLoaded(toPowerTextGroups(response.data.original))
						)
					)
				),
				switchAll()
			),
		initial as Loadable<Dictionary<(PowerTextProfile & { index: number })[]>, ReasonCode>,
		[classId] as const
	);

	return (
		<div className="p-8">
			<LoadableComponent
				data={powers}
				errorComponent={() => <>Not Found</>}
				loadedComponent={(loaded, isLoadingNext) => (
					<>
						<div className="storybook-md-theme">
							{Object.keys(loaded).map((header) => (
								<PowerSection header={header} key={header} powers={loaded[header]} classId={classId} />
							))}
						</div>
						{isLoadingNext ? <>Loading</> : null}
					</>
				)}
				loadingComponent={<>Loading</>}
			/>
		</div>
	);

	function toPowerTextGroups(data: ClassDetailsReadOnly) {
		return groupBy(
			(block) => `Level ${block.profile.level} ${block.profile.usage} Powers`,
			data.powers.map((p, index) => ({ ...p, index }))
		);
	}
}
