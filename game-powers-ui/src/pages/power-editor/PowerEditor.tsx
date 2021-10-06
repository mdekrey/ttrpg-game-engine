import { ClassDetailsReadOnly } from 'api/models/ClassDetailsReadOnly';
import { PowerTextProfile } from 'api/models/PowerTextProfile';
import { ReaderLayout } from 'components/reader-layout';
import { useApi } from 'core/hooks/useApi';
import { useObservable } from 'core/hooks/useObservable';
import { initial, Loadable, makeError, makeLoaded, makeLoading } from 'core/loadable/loadable';
import { LoadableComponent } from 'core/loadable/LoadableComponent';
import { Dictionary } from 'lodash';
import { groupBy } from 'lodash/fp';
import { BehaviorSubject, combineLatest } from 'rxjs';
import { delay, map, repeatWhen, switchAll, takeWhile } from 'rxjs/operators';
import useConstant from 'use-constant';
import { PowerSection } from './powers/PowerSection';

type ReasonCode = 'NotFound';

export function PowerEditor({ data: { classId } }: { data: { classId: string } }) {
	const restartPolling = useConstant(() => new BehaviorSubject<void>(undefined));
	const api = useApi();
	const data = useObservable(
		(input$) =>
			combineLatest([input$, restartPolling]).pipe(
				map(([inputs]) => inputs),
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
		initial as Loadable<{ name: string; powers: Dictionary<PowerTextProfile[]> }, ReasonCode>,
		[classId] as const
	);

	return (
		<div className="p-8">
			<LoadableComponent
				data={data}
				errorComponent={() => <>Not Found</>}
				loadedComponent={(loaded, isLoadingNext) => (
					<>
						<ReaderLayout>
							<h1 className="font-header font-bold mt-4 first:mt-0 text-theme text-2xl">{loaded.name}</h1>
							{Object.keys(loaded.powers).map((header) => (
								<PowerSection
									header={header}
									key={header}
									powers={loaded.powers[header]}
									classId={classId}
									onRequestReload={() => restartPolling.next()}
								/>
							))}
						</ReaderLayout>
						{isLoadingNext ? <>Loading</> : null}
					</>
				)}
				loadingComponent={<>Loading</>}
			/>
		</div>
	);

	function toPowerTextGroups(responseData: ClassDetailsReadOnly) {
		return {
			name: responseData.name,
			powers: groupBy((block) => `Level ${block.profile.level} ${block.profile.usage} Powers`, responseData.powers),
		};
	}
}
