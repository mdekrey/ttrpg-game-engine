import { useGameForm } from 'core/hooks/useGameForm';
import { Button } from 'components/button/Button';
import { Card } from 'components/card/card';
import { MultiselectField, NumberboxField, SelectField, TextboxField } from 'components/forms';
import { ListField } from 'components/forms/list-editor/ListEditor';
import { ButtonRow } from 'components/ButtonRow';
import { ClassProfile } from 'api/models/ClassProfile';
import { ToolProfile } from 'api/models/ToolProfile';
import { abilities, classSurveySchema, damageTypes, roles, toolRanges, toolTypes } from 'core/schemas/api';

const defaultToolProfile: Readonly<ToolProfile> = {
	toolType: 'Weapon',
	toolRange: 'Melee',
	abilities: [],
	preferredDamageTypes: [],
	powerProfileConfig: { modifierChances: [{ selector: '$', weight: 1 }], powerChances: [{ selector: '$', weight: 1 }] },
};

const powerSelectors = {
	$: 'Anything',
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
					removeLabel="Remove Tool"
					label="Tools"
					form={form}
					defaultItem={defaultToolProfile}
					name="tools"
					itemEditor={(path) => (
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
								depth={1}
								className="col-span-6 sm:col-span-3"
								addLabel="Add Modifier Chance"
								removeLabel="Remove Modifier Chance"
								label="Modifier Chances"
								form={form}
								defaultItem={{ selector: '$', weight: 1 }}
								name={`${path}.powerProfileConfig.modifierChances`}
								itemEditor={(path2) => {
									return (
										<div className="grid grid-cols-6 gap-6">
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
											<NumberboxField
												label="Weight"
												className="col-span-6 sm:col-span-2"
												form={form}
												name={`${path2}.weight`}
											/>
										</div>
									);
								}}
							/>
							<ListField
								depth={1}
								className="col-span-6 sm:col-span-3"
								addLabel="Add Power Chance"
								removeLabel="Remove Power Chance"
								label="Power Chances"
								form={form}
								defaultItem={{ selector: '$', weight: 1 }}
								name={`${path}.powerProfileConfig.powerChances`}
								itemEditor={(path2) => (
									<div className="grid grid-cols-6 gap-6">
										<SelectField
											label="Selector"
											className="col-span-6 sm:col-span-4"
											form={form}
											name={`${path2}.selector`}
											options={Object.keys(powerSelectors)}
											optionDisplay={(selector) => powerSelectors[selector as keyof typeof powerSelectors] || selector}
										/>
										<NumberboxField
											label="Weight"
											className="col-span-6 sm:col-span-2"
											form={form}
											name={`${path2}.weight`}
										/>
									</div>
								)}
							/>
						</div>
					)}
				/>
				<ButtonRow className="col-span-6">
					<Button type="submit">Submit</Button>
				</ButtonRow>
			</Card>
		</form>
	);
}
