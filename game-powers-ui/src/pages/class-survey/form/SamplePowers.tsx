/* eslint-disable react/no-array-index-key */
import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { ClassProfile } from 'api/models/ClassProfile';
import { useApi } from 'core/hooks/useApi';
import { Subject } from 'rxjs';
import useConstant from 'use-constant';
import { filter } from 'rxjs/operators';
import { StructuredResponses } from 'api/operations/generateSamplePower';
import { PowerTextBlock } from 'components/power';
import { PowerType } from 'components/power/Power';

function is200<T extends { statusCode: number | 'other' }>(
	response: T
): response is T extends { statusCode: 200 } ? T : never {
	return response.statusCode === 200;
}

export function SamplePowers({
	classProfile,
	toolIndex,
	powerProfileIndex,
}: {
	classProfile: ClassProfile;
	toolIndex: number;
	powerProfileIndex: number;
}) {
	const scrollEvent$ = useConstant(() => new Subject<typeof generateParams>());

	const divRef = useRef<HTMLDivElement>(null);
	const api = useApi();
	const generateParams = useMemo(
		() => ({
			classProfile,
			toolIndex,
			powerProfileIndex,
			level: 19,
			usage: 'Daily' as const,
		}),
		[classProfile, toolIndex, powerProfileIndex]
	);
	const [powers, setPowers] = useState<StructuredResponses[200]['application/json'][]>([]);

	const shouldExpand = useCallback(
		() =>
			divRef.current !== null &&
			divRef.current.scrollLeft + divRef.current.clientWidth * 1.5 > divRef.current.scrollWidth,
		[]
	);

	const addPower = useCallback(
		async (params: typeof generateParams) => {
			const response = await api.generateSamplePower({}, params, 'application/json').toPromise();
			if (is200(response)) setPowers((p) => [...p, response.data]);
			if (shouldExpand()) addPower(params);
		},
		[api, shouldExpand]
	);

	useEffect(() => {
		setPowers([]);
		addPower(generateParams);
	}, [addPower, generateParams]);

	useEffect(() => {
		let running = false;

		const subscription = scrollEvent$.pipe(filter(shouldExpand)).subscribe(async (p) => {
			if (running || !shouldExpand()) return;
			running = true;
			try {
				await addPower(p);
			} catch (ex) {
				// intentionally blank exception
			}
			running = false;
		}); // TODO - call addPower
		return () => subscription.unsubscribe();
	}, [scrollEvent$, addPower, shouldExpand]);

	return (
		<div
			className="flex flex-row overflow-x-auto items-start mt-4"
			ref={divRef}
			onScroll={() => scrollEvent$.next(generateParams)}>
			{powers.map((power, i) => (
				<button type="button" key={i} className="flex-shrink-0 w-96 mr-4 text-left">
					<PowerTextBlock
						{...power.power}
						powerUsage={power.power.powerUsage as PowerType}
						attackType={(power.power.attackType || null) as 'Personal' | 'Ranged' | 'Melee' | 'Close' | 'Area' | null}
					/>
				</button>
			))}
		</div>
	);
}
