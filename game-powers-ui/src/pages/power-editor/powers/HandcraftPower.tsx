/* eslint-disable react/no-array-index-key */
import { useState } from 'react';
import { map, startWith, switchAll } from 'rxjs/operators';
import { PowerHighLevelInfo } from 'api/models/PowerHighLevelInfo';
import { PowerProfileChoice } from 'api/models/PowerProfileChoice';
import { PowerTextBlock } from 'components/power';
import { powerTextBlockToProps } from 'components/power/PowerTextBlock';
import { useApi } from 'core/hooks/useApi';
import { useObservable } from 'core/hooks/useObservable';
import { initial, makeError, makeLoaded, makeLoading } from 'core/loadable/loadable';
import { LoadableComponent } from 'core/loadable/LoadableComponent';
import { PowerGeneratorState } from 'api/models/PowerGeneratorState';

export type HandcraftPowerProps = PowerHighLevelInfo & {};

export const HandcraftPower = ({ classProfile, level, usage, toolIndex, powerProfileIndex }: HandcraftPowerProps) => {
	const api = useApi();
	const [lastChoice, setLastChoice] = useState<null | [PowerProfileChoice, PowerGeneratorState]>(null);

	const data = useObservable(
		(input$) =>
			input$.pipe(
				// eslint-disable-next-line @typescript-eslint/no-shadow
				map(([classProfile, level, usage, toolIndex, powerProfileIndex, lastChoice]) =>
					(lastChoice === null
						? api.beginPowerGeneration({ body: { classProfile, level, usage, toolIndex, powerProfileIndex } })
						: api.continuePowerGeneration({
								body: { profile: lastChoice[0].profile, state: lastChoice[1] },
						  })
					).pipe(
						map((response) => (response.statusCode === 200 ? makeLoaded(response.data) : makeError('Invalid'))),
						startWith(makeLoading())
					)
				),
				switchAll()
			),
		initial,
		[classProfile, level, usage, toolIndex, powerProfileIndex, lastChoice] as const
	);

	return (
		<div>
			<LoadableComponent
				data={data}
				errorComponent={() => <>Not Found</>}
				loadedComponent={(loaded) => {
					return (
						<div className="grid gap-2 lg:grid-cols-3">
							{loaded.choices.map((choice, index) => (
								<button
									type="button"
									key={index}
									className="text-left"
									onClick={() => setLastChoice([choice, loaded.state])}>
									<PowerTextBlock {...powerTextBlockToProps(choice.text)} />
								</button>
							))}
						</div>
					);
				}}
				loadingComponent={<>Loading</>}
			/>
		</div>
	);
};
