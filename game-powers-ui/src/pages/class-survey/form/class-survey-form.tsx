import { MinusIcon } from '@heroicons/react/solid';
import { useGameForm } from 'core/hooks/useGameForm';
import { Button } from 'components/button/Button';
import { Card } from 'components/card/card';
import { MultiselectField, NumberboxField, SelectField, TextboxField } from 'components/forms';
import { ListField } from 'components/forms/list-editor/ListEditor';
import { ButtonRow } from 'components/ButtonRow';
import { ClassProfile } from 'api/models/ClassProfile';
import { ToolProfile } from 'api/models/ToolProfile';
import { abilities, classSurveySchema, damageTypes, roles, toolRanges, toolTypes } from 'core/schemas/api';
import { powerSelectors } from './powerSelectors';

const defaultToolProfile: Readonly<ToolProfile> = {
	toolType: 'Weapon',
	toolRange: 'Melee',
	abilities: ['Strength'],
	preferredDamageTypes: ['Normal'],
	powerProfileConfig: {
		modifierChances: [{ selector: '$', weight: 1.0 }],
		powerChances: [
			{ selector: "$..[?(@.Name=='Non-Armor Defense' || @.Name=='To-Hit Bonus to Current Attack')]", weight: 1.0 },
			{ selector: "$..[?(@.Name=='RequiredHitForNextAttack')]", weight: 1.0 },
			{ selector: "$..[?(@.Name=='RequiresPreviousHit')]", weight: 1.0 },
			{ selector: "$..[?(@.Name=='TwoHits')]", weight: 1.0 },
			{ selector: "$..[?(@.Name=='UpToThreeTargets')]", weight: 1.0 },
			{ selector: "$..[?(@.Name=='Multiattack')]", weight: 1.0 },
			{ selector: "$..[?(@.Name=='Condition')]", weight: 1.0 },
			{ selector: "$..[?(@.Name=='OpportunityAction')]", weight: 1.0 },
			{ selector: "$..[?(@.Name=='Skirmish Movement')]", weight: 1.0 },
		],
	},
};

const modifierSelectors = {
	$: 'Anything',
};

export function ClassSurveyForm({
	className,
	onSubmit,
	defaultValues,
}: {
	className?: string;
	onSubmit?: (form: ClassProfile) => void;
	defaultValues?: ClassProfile;
}) {
	const { handleSubmit, ...form } = useGameForm<ClassProfile>({
		defaultValues: defaultValues || {
			name: 'Custom Class',
			role: 'Controller',
			powerSource: 'Martial',
			tools: [defaultToolProfile],
		},
		schema: classSurveySchema,
	});

	return (
		<form className={className} onSubmit={onSubmit && handleSubmit(onSubmit)}>
			<Card className="grid grid-cols-6 gap-6">
				<TextboxField label="Class Name" className="col-span-6 sm:col-span-3" form={form} name="name" />
				<SelectField className="col-span-6 sm:col-span-3" label="Role" form={form} name="role" options={roles} />
				<TextboxField label="PowerSource" className="col-span-6 sm:col-span-3" form={form} name="powerSource" />
				<ListField
					className="col-span-6"
					addLabel="Add Tool"
					label="Tools"
					form={form}
					defaultNewItem={() => defaultToolProfile}
					name="tools"
					itemEditor={(path, removeTool) => (
						<Card depth={1} className="my-3">
							<div className="grid grid-cols-6 gap-6">
								<SelectField
									className="col-span-6 sm:col-span-3"
									label="Tool"
									form={form}
									name={`${path}.toolType`}
									options={toolTypes}
								/>
								<SelectField
									className="col-span-6 sm:col-span-3"
									label="Range"
									form={form}
									name={`${path}.toolRange`}
									options={toolRanges}
								/>
								<MultiselectField
									className="col-span-6 sm:col-span-3"
									label="Abilities"
									form={form}
									name={`${path}.abilities`}
									options={abilities}
								/>
								<MultiselectField
									className="col-span-6 sm:col-span-3"
									label="Damage Types"
									form={form}
									name={`${path}.preferredDamageTypes`}
									options={damageTypes}
								/>
								<ListField
									className="col-span-6 sm:col-span-3"
									addLabel="Add Power Chance"
									label="Power Chances"
									form={form}
									defaultNewItem={() => ({ selector: '$', weight: 1 })}
									name={`${path}.powerProfileConfig.powerChances`}
									itemEditor={(path2, removeItem) => (
										<div className="grid grid-cols-6 gap-6 my-3">
											<SelectField
												label="Selector"
												className="col-span-6 sm:col-span-4"
												form={form}
												name={`${path2}.selector`}
												options={powerSelectors.map((p) => p.selector)}
												optionDisplay={(selector) =>
													powerSelectors.find((p) => p.selector === selector)?.name || selector
												}
											/>
											<ButtonRow className="col-span-6 sm:col-span-2" reversed={false}>
												<NumberboxField label="Weight" className="flex-grow" form={form} name={`${path2}.weight`} />

												<Button
													type="button"
													look="cancel"
													contents="icon"
													onClick={removeItem}
													title="Remove Power Chance">
													<MinusIcon className="h-em w-em" />
												</Button>
											</ButtonRow>
										</div>
									)}
								/>
								<ListField
									className="col-span-6 sm:col-span-3"
									addLabel="Add Modifier Chance"
									label="Modifier Chances"
									form={form}
									defaultNewItem={() => ({ selector: '$', weight: 1 })}
									name={`${path}.powerProfileConfig.modifierChances`}
									itemEditor={(path2, removeItem) => {
										return (
											<div className="grid grid-cols-6 gap-6 my-3">
												<SelectField
													label="Selector"
													className="col-span-6 sm:col-span-4"
													form={form}
													name={`${path2}.selector`}
													options={Object.keys(modifierSelectors)}
													optionDisplay={(selector) =>
														modifierSelectors[selector as keyof typeof modifierSelectors] || selector
													}
												/>
												<ButtonRow className="col-span-6 sm:col-span-2" reversed={false}>
													<NumberboxField label="Weight" className="flex-grow" form={form} name={`${path2}.weight`} />

													<Button
														type="button"
														look="cancel"
														contents="icon"
														onClick={removeItem}
														title="Remove Modifier Chance">
														<MinusIcon className="h-em w-em" />
													</Button>
												</ButtonRow>
											</div>
										);
									}}
								/>
								<ButtonRow className="col-span-6">
									<Button look="cancel" onClick={removeTool}>
										Remove Tool
									</Button>
								</ButtonRow>
							</div>
						</Card>
					)}
				/>
				<ButtonRow className="col-span-6">
					<Button type="submit">Submit</Button>
				</ButtonRow>
			</Card>
		</form>
	);
}
