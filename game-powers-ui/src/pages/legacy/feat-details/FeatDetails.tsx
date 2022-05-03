import { of } from 'rxjs';
import { map, switchAll } from 'rxjs/operators';
import { useApi } from 'core/hooks/useApi';
import { useObservable } from 'core/hooks/useObservable';
import { useMemoizeObservable } from 'core/hooks/useMemoizeObservable';
import { initial, Loadable, makeError, makeLoaded } from 'core/loadable/loadable';
import { LoadableComponent } from 'core/loadable/LoadableComponent';
import { StructuredResponses } from 'api/operations/getLegacyFeat';
import { ReaderLayout } from 'components/reader-layout';
import { DynamicMarkdown } from 'components/mdx/DynamicMarkdown';
import { LegacyFeatDetails } from 'api/models/LegacyFeatDetails';
import { wizardsTextToMarkdown } from '../wizards-text-to-markdown';
import { DisplayPower } from '../display-power';
import { Sources } from '../sources';

type ReasonCode = 'NotFound';

export function FeatDetails({
	data: { id, details: preloadedFeatDetails },
}: {
	data: { id: string; details?: LegacyFeatDetails };
}) {
	const api = useApi();
	const featId$ = useMemoizeObservable([id, preloadedFeatDetails] as const);
	const data = useObservable(
		() =>
			featId$.pipe(
				// eslint-disable-next-line @typescript-eslint/no-shadow
				map(([id, featDetails]) =>
					featDetails
						? of(makeLoaded(featDetails))
						: api
								.getLegacyFeat({ params: { id } })
								.pipe(
									map((response) =>
										response.statusCode === 404 ? makeError<ReasonCode>('NotFound' as const) : makeLoaded(response.data)
									)
								)
				),
				switchAll()
			),
		initial as Loadable<StructuredResponses[200]['application/json'], ReasonCode>
	);

	return (
		<ReaderLayout>
			<LoadableComponent
				data={data}
				errorComponent={() => <>Not Found</>}
				loadedComponent={({ details, prerequisites, powers }) => (
					<>
						<h1 className="font-header font-bold mt-4 first:mt-0 text-theme text-3xl">
							{details.name} <Sources sources={details.sources} />
						</h1>
						<DynamicMarkdown contents={wizardsTextToMarkdown(details.description, { depth: 1 })} />
						{prerequisites && <DynamicMarkdown contents={`**Prerequisites:** ${prerequisites}`} />}
						{powers.map((power, powerIndex) => (
							<DisplayPower details={power} key={powerIndex} />
						))}
					</>
				)}
				loadingComponent={<>Loading</>}
			/>
		</ReaderLayout>
	);
}
