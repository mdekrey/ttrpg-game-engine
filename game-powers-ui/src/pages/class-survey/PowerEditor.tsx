import { ClassDetailsReadOnly } from 'api/models/ClassDetailsReadOnly';
import { PowerTextProfile } from 'api/models/PowerTextProfile';
import classNames from 'classnames';
import { Card } from 'components/card/card';
import { PowerTextBlock } from 'components/power';
import { PowerType } from 'components/power/Power';
import { useApi } from 'core/hooks/useApi';
import { useObservable } from 'core/hooks/useObservable';
import { initial, Loadable, makeError, makeLoaded, makeLoading } from 'core/loadable/loadable';
import { LoadableComponent } from 'core/loadable/LoadableComponent';
import { Dictionary } from 'lodash';
import { groupBy } from 'lodash/fp';
import { delay, map, repeatWhen, switchAll, takeWhile } from 'rxjs/operators';

function PowerSection({ header, powers }: { header: string; powers: PowerTextProfile[] }) {
	return (
		<section>
			{powers.map((p, i) => {
				const { powerUsage, attackType, ...text } = p.text;
				return (
					// eslint-disable-next-line react/no-array-index-key
					<article className="mb-4" style={{ pageBreakInside: 'avoid' }} key={i}>
						{i === 0 && (
							<h2 className={classNames('font-header font-bold', 'mt-4 first:mt-0', 'text-theme text-xl')}>{header}</h2>
						)}
						<PowerTextBlock
							{...text}
							powerUsage={powerUsage as PowerType}
							attackType={attackType as 'Personal' | 'Ranged' | 'Melee' | 'Close' | 'Area' | null}
						/>
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
		initial as Loadable<Dictionary<PowerTextProfile[]>, ReasonCode>,
		[classId] as const
	);

	return (
		<div className="p-8 bg-gray-50 min-h-screen">
			<LoadableComponent
				data={powers}
				errorComponent={() => <>Not Found</>}
				loadedComponent={(loaded, isLoadingNext) => (
					<Card className="mt-4">
						<div className="storybook-md-theme">
							{Object.keys(loaded).map((header) => (
								<PowerSection header={header} key={header} powers={loaded[header]} />
							))}
						</div>
						{isLoadingNext ? <>Loading</> : null}
					</Card>
				)}
				loadingComponent={<>Loading</>}
			/>
		</div>
	);

	function toPowerTextGroups(data: ClassDetailsReadOnly) {
		return groupBy((block) => `Level ${block.profile.level} ${block.profile.usage} Powers`, data.powers);
	}
}
