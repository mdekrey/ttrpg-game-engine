import * as yup from 'yup';
import { useGameForm } from 'core/hooks/useGameForm';
import { Button } from 'components/button/Button';
import { Card } from 'components/card/card';
import { MultiselectField, SelectField, TextboxField } from 'components/forms';
import { ListField } from 'components/forms/list-editor/ListEditor';
import { ButtonRow } from '../../components/ButtonRow';

type CharacterRole = 'Controller' | 'Defender' | 'Leader' | 'Striker';
type ToolType = 'Weapon' | 'Implement';
type ToolRange = 'Melee' | 'Range';
type Ability = 'Strength' | 'Constitution' | 'Dexterity' | 'Intelligence' | 'Wisdom' | 'Charisma';
type DamageType = 'Normal' | 'Fire' | 'Cold' | 'Necrotic' | 'Radiant' | 'Lightning' | 'Thunder' | 'Poison' | 'Force';

type ModifierChance = {
	selector: string;
	weight: number;
};

type PowerProfileConfig = {
	modifierChances: ModifierChance[];
	powerChances: ModifierChance[];
};

type ToolSurveyForm = {
	toolType: ToolType;
	toolRange: ToolRange;
	abilities: Ability[];
	preferredDamageTypes: DamageType[];
	powerProfileConfig: PowerProfileConfig;
};

type ClassSurveyForm = {
	name: string;
	role: CharacterRole;
	powerSource: string;
	tools: ToolSurveyForm[];
};

const roles: CharacterRole[] = ['Controller', 'Defender', 'Leader', 'Striker'];
const toolTypes: ToolType[] = ['Weapon', 'Implement'];
const toolRanges: ToolRange[] = ['Melee', 'Range'];
const abilities: Ability[] = ['Strength', 'Constitution', 'Dexterity', 'Intelligence', 'Wisdom', 'Charisma'];
const damageTypes: DamageType[] = [
	'Normal',
	'Fire',
	'Cold',
	'Necrotic',
	'Radiant',
	'Lightning',
	'Thunder',
	'Poison',
	'Force',
];

const abilitySchema = yup.mixed().oneOf(abilities).required().defined() as yup.SchemaOf<Ability>;
const damageTypeSchema = yup.string().oneOf(damageTypes).required().defined() as yup.SchemaOf<DamageType>;
const modifierChanceSchema: yup.SchemaOf<ModifierChance> = yup.object({
	selector: yup.string().required(),
	weight: yup.number().required().positive(),
});
const powerProfileConfigSchema: yup.SchemaOf<PowerProfileConfig> = yup.object({
	modifierChances: yup.array(modifierChanceSchema).min(1).label('Modifier Chances'),
	powerChances: yup.array(modifierChanceSchema).min(1).label('Power Chances'),
});
const toolSurveySchema: yup.SchemaOf<ToolSurveyForm> = yup.object({
	toolType: yup.mixed<ToolType>().oneOf(toolTypes).required().label('Tool Type'),
	toolRange: yup.mixed<ToolRange>().oneOf(toolRanges).required().label('Tool Range'),
	abilities: yup.array(abilitySchema).min(1).label('Abilities'),
	preferredDamageTypes: yup
		.array(damageTypeSchema)
		.min(1, 'Must have at least one damage type')
		.label('Preferred Damage Types'),
	powerProfileConfig: powerProfileConfigSchema,
});
const classSurveySchema: yup.SchemaOf<ClassSurveyForm> = yup.object({
	name: yup.string().required().label('Name'),
	role: yup.mixed<CharacterRole>().oneOf(roles).required().label('Role'),
	powerSource: yup.string().required().label('Power Source'),
	tools: yup.array(toolSurveySchema).min(1, 'Must have at least one tool'),
});

const defaultToolProfile: Readonly<ToolSurveyForm> = {
	toolType: 'Weapon',
	toolRange: 'Melee',
	abilities: [],
	preferredDamageTypes: [],
	powerProfileConfig: { modifierChances: [{ selector: '$', weight: 1 }], powerChances: [{ selector: '$', weight: 1 }] },
};

export function ClassSurvey({
	className,
	onSubmit,
}: {
	className?: string;
	onSubmit?: (form: ClassSurveyForm) => void;
}) {
	const { handleSubmit, ...form } = useGameForm<ClassSurveyForm>({
		defaultValues: {
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
					itemEditor={(index) => (
						<div className="grid grid-cols-6 gap-6">
							<SelectField
								className="col-span-6 sm:col-span-3"
								label="Tool"
								form={form}
								name={`tools.${index}.toolType`}
								options={toolTypes}
							/>
							<SelectField
								className="col-span-6 sm:col-span-3"
								label="Range"
								form={form}
								name={`tools.${index}.toolRange`}
								options={toolRanges}
							/>
							<MultiselectField
								className="col-span-6 sm:col-span-3"
								label="Abilities"
								form={form}
								name={`tools.${index}.abilities`}
								options={abilities}
							/>
							<MultiselectField
								className="col-span-6 sm:col-span-3"
								label="Damage Types"
								form={form}
								name={`tools.${index}.preferredDamageTypes`}
								options={damageTypes}
							/>
						</div>
					)}
				/>
				<pre className="col-span-6">{JSON.stringify(form.formState.errors, undefined, 4)}</pre>
				<ButtonRow className="col-span-6">
					<Button type="submit">Submit</Button>
				</ButtonRow>
			</Card>
		</form>
	);
}
