/* eslint-disable react/no-array-index-key */
import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { ClassProfile } from 'api/models/ClassProfile';
import { useApi } from 'core/hooks/useApi';
import { Subject } from 'rxjs';
import useConstant from 'use-constant';
import { filter } from 'rxjs/operators';
import { PowerTextBlock } from 'components/power';
import { PowerType } from 'components/power/Power';
import { PowerFrequency } from 'api/models/PowerFrequency';
import { PowerTextBlock as PowerTextBlockType } from 'api/models/PowerTextBlock';

function is200<T extends { statusCode: number | 'other' }>(
	response: T
): response is T extends { statusCode: 200 } ? T : never {
	return response.statusCode === 200;
}

export type SamplePowerData = {
	power: PowerTextBlockType;
	powerJson: string;
};

export function SamplePowers({
	classProfile,
	toolIndex,
	powerProfileIndex,
	level,
	usage,
	onSelectPower,
}: {
	classProfile: ClassProfile;
	toolIndex: number;
	powerProfileIndex: number;
	level: number;
	usage: PowerFrequency;
	onSelectPower?: (power: SamplePowerData) => void;
}) {
	const scrollEvent$ = useConstant(() => new Subject<typeof generateParams>());

	const divRef = useRef<HTMLDivElement>(null);
	const api = useApi();
	const generateParams = useMemo(
		() => ({
			classProfile,
			toolIndex,
			powerProfileIndex,
			level,
			usage,
		}),
		[classProfile, toolIndex, powerProfileIndex, level, usage]
	);
	const [powers, setPowers] = useState<SamplePowerData[]>([]);

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
			className="flex flex-row overflow-x-auto items-start"
			ref={divRef}
			onScroll={() => scrollEvent$.next(generateParams)}>
			{powers.map((power, i) => (
				<button
					type="button"
					key={i}
					className="flex-shrink-0 w-96 max-w-full mr-4 text-left"
					onClick={onSelectPower && (() => onSelectPower(power))}>
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
