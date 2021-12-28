import { ClassDetailsReadOnly } from 'api/models/ClassDetailsReadOnly';
import { PowerTextProfile } from 'api/models/PowerTextProfile';
import { ReaderLayout } from 'components/reader-layout';
import { useApi } from 'core/hooks/useApi';
import { useObservable } from 'core/hooks/useObservable';
import { initial, Loadable, makeError, makeLoaded } from 'core/loadable/loadable';
import { LoadableComponent } from 'core/loadable/LoadableComponent';
import { Dictionary } from 'lodash';
import { groupBy } from 'lodash/fp';
import { map, switchAll } from 'rxjs/operators';
import { PowerSection } from './powers/PowerSection';

type ReasonCode = 'NotFound';

export function ClassViewer({ data: { classId } }: { data: { classId: string } }) {
	const api = useApi();
	const data = useObservable(
		(input$) =>
			input$.pipe(
				map(([id]) =>
					api
						.getClass({ params: { id } })
						.pipe(
							map((response) =>
								response.statusCode === 404
									? makeError<ReasonCode>('NotFound' as const)
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
								<PowerSection header={header} key={header} powers={loaded.powers[header]} />
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
			powers: groupBy(
				(block) => `${block.profile.level && `Level ${block.profile.level} `}${block.profile.usage} Powers`,
				responseData.powers
			),
		};
	}
}
