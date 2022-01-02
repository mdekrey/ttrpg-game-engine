/* eslint-disable react/no-array-index-key */
import { useState } from 'react';
import { map, switchAll } from 'rxjs/operators';
import { CheckIcon, XIcon } from '@heroicons/react/solid';
import { PowerHighLevelInfo } from 'api/models/PowerHighLevelInfo';
import { PowerProfileChoice } from 'api/models/PowerProfileChoice';
import { PowerProfile } from 'api/models/PowerProfile';
import { ButtonRow } from 'components/ButtonRow';
import { Button } from 'components/button/Button';
import { PowerTextBlock } from 'components/power';
import { powerTextBlockToProps } from 'components/power/PowerTextBlock';
import { useApi } from 'core/hooks/useApi';
import { useObservable } from 'core/hooks/useObservable';
import { initial, isLoaded, makeError, makeLoaded } from 'core/loadable/loadable';
import { LoadableComponent } from 'core/loadable/LoadableComponent';
import { PowerGeneratorState } from 'api/models/PowerGeneratorState';
import { YamlEditor } from 'components/monaco/YamlEditor';
import { RequestBodies } from 'api/operations/replacePowerWith';

export type SelectPowerResult = RequestBodies['application/json'];

export type HandcraftPowerProps = PowerHighLevelInfo & {
	onSelectPower?: (power: SelectPowerResult) => void;
	onCancel?: () => void;
};

export const HandcraftPower = ({
	classProfile,
	level,
	usage,
	toolIndex,
	powerProfileIndex,
	onSelectPower,
	onCancel,
}: HandcraftPowerProps) => {
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
					).pipe(map((response) => (response.statusCode === 200 ? makeLoaded(response.data) : makeError('Invalid'))))
				),
				switchAll()
			),
		initial,
		[classProfile, level, usage, toolIndex, powerProfileIndex, lastChoice] as const
	);

	const updatePower = (power: PowerProfile) =>
		setLastChoice((v) =>
			v
				? [{ ...v[0], profile: power }, v[1]]
				: isLoaded(data)
				? [{ ...data.value.finalizedChoice, profile: power }, data.value.state]
				: null
		);

	return (
		<div className="grid gap-8">
			<LoadableComponent
				data={data}
				errorComponent={() => <>Not Found</>}
				loadedComponent={(loaded) => {
					return (
						<>
							<div className="grid grid-cols-2 gap-2">
								<YamlEditor value={loaded.state.powerProfile} onChange={updatePower} />
								<div className="flex flex-col justify-between">
									<PowerTextBlock {...powerTextBlockToProps(loaded.finalizedChoice.text)} />
									<ButtonRow>
										{onSelectPower && (
											<Button
												contents="icon"
												look="primary"
												onClick={() =>
													onSelectPower({
														profile: loaded.finalizedChoice.profile,
														powerInfo: (lastChoice ? lastChoice[1] : loaded.state).buildContext.powerInfo,
														flavorText: (lastChoice ? lastChoice[1] : loaded.state).flavorText,
													})
												}>
												<CheckIcon />
											</Button>
										)}
										{onCancel && (
											<Button contents="icon" look="primary" onClick={onCancel}>
												<XIcon />
											</Button>
										)}
									</ButtonRow>
								</div>
							</div>
							<div className="grid gap-2 lg:grid-cols-3 items-start">
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
						</>
					);
				}}
				loadingComponent={<>Loading</>}
			/>
		</div>
	);
};
