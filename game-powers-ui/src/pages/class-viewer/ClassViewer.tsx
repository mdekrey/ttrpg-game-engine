import { PencilIcon } from '@heroicons/react/solid';
import { ClassDetailsReadOnly } from 'src/api/models/ClassDetailsReadOnly';
import { StructuredResponses } from 'src/api/operations/getClass';
import { ReaderLayout } from 'src/components/reader-layout';
import { useApi } from 'src/core/hooks/useApi';
import { useObservable } from 'src/core/hooks/useObservable';
import { initial, Loadable, makeError, makeLoaded } from 'src/core/loadable/loadable';
import { LoadableComponent } from 'src/core/loadable/LoadableComponent';
import { groupBy } from 'lodash/fp';
import { map, switchAll } from 'rxjs/operators';
import { ClassDescription } from '../../components/mdx/ClassDescription';
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
								response.statusCode === 404 ? makeError<ReasonCode>('NotFound' as const) : makeLoaded(response.data)
							)
						)
				),
				switchAll()
			),
		initial as Loadable<StructuredResponses[200]['application/json'], ReasonCode>,
		[classId] as const
	);

	return (
		<div className="p-8">
			<LoadableComponent
				data={data}
				errorComponent={() => <>Not Found</>}
				loadedComponent={(loaded, isLoadingNext) => {
					const powers = toPowerTextGroups(loaded.original);
					return (
						<>
							<ReaderLayout>
								<h1 className="font-header font-bold mt-4 first:mt-0 text-theme text-3xl">
									{loaded.original.name}
									{loaded.state !== 'Read-Only' ? (
										<a href={`/class/edit/${classId}`} className="underline text-theme">
											<PencilIcon className="w-5 h-5 inline-block" />
										</a>
									) : null}
								</h1>
								<ClassDescription mdx={loaded.original.description || ''} />
								{Object.keys(powers).map((header) => (
									<PowerSection header={header} key={header} powers={powers[header]} />
								))}
							</ReaderLayout>
							{isLoadingNext ? <>Loading</> : null}
						</>
					);
				}}
				loadingComponent={<>Loading</>}
			/>
		</div>
	);

	function toPowerTextGroups(responseData: ClassDetailsReadOnly) {
		return groupBy((block) => `${block.level && `Level ${block.level} `}${block.usage} Powers`, responseData.powers);
	}
}
