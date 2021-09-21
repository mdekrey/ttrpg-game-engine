import { ClassDetailsReadOnly } from 'api/models/ClassDetailsReadOnly';
import { PowerTextProfile } from 'api/models/PowerTextProfile';
import { useApi } from 'core/hooks/useApi';
import { useObservable } from 'core/hooks/useObservable';
import { initial, Loadable, makeError, makeLoaded, makeLoading } from 'core/loadable/loadable';
import { LoadableComponent } from 'core/loadable/LoadableComponent';
import { Dictionary } from 'lodash';
import { groupBy } from 'lodash/fp';
import { delay, map, repeatWhen, switchAll, takeWhile } from 'rxjs/operators';
import { PowerSection } from './powers/PowerSection';

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
