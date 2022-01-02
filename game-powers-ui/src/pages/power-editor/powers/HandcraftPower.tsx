/* eslint-disable react/no-array-index-key */
import { useMemo, useState } from 'react';
import { map, switchAll } from 'rxjs/operators';
import { CheckIcon, XIcon } from '@heroicons/react/solid';
import * as yup from 'yup';
import { ObjectShape } from 'yup/lib/object';
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
import { useGameForm } from 'core/hooks/useGameForm';
import { TextboxField } from 'components/forms';

export type SelectPowerResult = RequestBodies['application/json'];
type FlavorText = Record<string, string>;

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
	const [lastChoice, setLastChoice] = useState<null | [PowerProfileChoice, PowerGeneratorState, boolean]>(null);

	const data = useObservable(
		(input$) =>
			input$.pipe(
				// eslint-disable-next-line @typescript-eslint/no-shadow
				map(([classProfile, level, usage, toolIndex, powerProfileIndex, lastChoice]) =>
					(lastChoice === null
						? api.beginPowerGeneration({ body: { classProfile, level, usage, toolIndex, powerProfileIndex } })
						: api.continuePowerGeneration({
								body: {
									profile: lastChoice[0].profile,
									state: { ...lastChoice[1], flavorText: lastChoice[0].flavorText },
									advance: lastChoice[2],
								},
						  })
					).pipe(map((response) => (response.statusCode === 200 ? makeLoaded(response.data) : makeError('Invalid'))))
				),
				switchAll()
			),
		initial,
		[classProfile, level, usage, toolIndex, powerProfileIndex, lastChoice] as const
	);

	const keys = useMemo(() => {
		const flavorText = lastChoice
			? lastChoice[0].flavorText
			: data && isLoaded(data)
			? data.value.finalizedChoice.flavorText
			: {};
		return Object.keys(flavorText).sort();
	}, [lastChoice, data]);
	const flavorSchema = useMemo(() => {
		const result: ObjectShape = {};
		keys.forEach((key) => {
			result[key] = yup.string().required().label(key);
		});
		return yup.object(result) as yup.SchemaOf<FlavorText>;
	}, [keys]);

	const { handleSubmit, ...form } = useGameForm<FlavorText>({
		defaultValues: {},
		schema: flavorSchema,
	});

	const updatePower = (power: PowerProfile) =>
		setLastChoice((v) =>
			v
				? [{ ...v[0], profile: power }, v[1], false]
				: isLoaded(data)
				? [{ ...data.value.finalizedChoice, profile: power }, data.value.state, false]
				: null
		);
	const updateFlavorText = (flavorText: FlavorText) =>
		setLastChoice((v) =>
			v
				? [{ ...v[0], flavorText }, v[1], false]
				: isLoaded(data)
				? [
						{ ...data.value.finalizedChoice, profile: data.value.state.powerProfile, flavorText },
						data.value.state,
						false,
				  ]
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
							<form
								onSubmit={handleSubmit(updateFlavorText)}
								className="grid grid-cols-1 gap-3 self-start print:hidden">
								{keys.map((key) => (
									<TextboxField key={key} label={key} form={form} name={key} />
								))}
								<ButtonRow>
									<Button contents="icon" type="submit">
										<CheckIcon />
									</Button>
								</ButtonRow>
							</form>
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
														flavorText: (lastChoice ? lastChoice[0] : loaded.state).flavorText,
														powerInfo: (lastChoice ? lastChoice[1] : loaded.state).buildContext.powerInfo,
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
										onClick={() => setLastChoice([choice, loaded.state, true])}>
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
