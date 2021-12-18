import * as yup from 'yup';
import { Ability } from 'api/models/Ability';
import { CharacterRole } from 'api/models/CharacterRole';
import { ClassProfile } from 'api/models/ClassProfile';
import { DamageType } from 'api/models/DamageType';
import { PowerChance } from 'api/models/PowerChance';
import { PowerProfileConfig } from 'api/models/PowerProfileConfig';
import { ToolProfile } from 'api/models/ToolProfile';
import { ToolRange } from 'api/models/ToolRange';
import { ToolType } from 'api/models/ToolType';

export const roles: CharacterRole[] = ['Controller', 'Defender', 'Leader', 'Striker'];
export const toolTypes: ToolType[] = ['Weapon', 'Implement'];
export const toolRanges: ToolRange[] = ['Melee', 'Range'];
export const abilities: Ability[] = ['Strength', 'Constitution', 'Dexterity', 'Intelligence', 'Wisdom', 'Charisma'];
export const damageTypes: DamageType[] = [
	'Fire',
	'Cold',
	'Necrotic',
	'Radiant',
	'Lightning',
	'Thunder',
	'Poison',
	'Force',
];

export const abilitySchema = yup.mixed().oneOf(abilities).required().defined() as yup.SchemaOf<Ability>;
export const damageTypeSchema = yup.string().oneOf(damageTypes).required().defined() as yup.SchemaOf<DamageType>;
export const modifierChanceSchema: yup.SchemaOf<PowerChance> = yup.object({
	selector: yup.string().required().label('Selector'),
	weight: yup
		.number()
		.label('Weight')
		.required()
		.test({
			name: 'positive-or-zero',
			test: (v) => v !== undefined && v >= 0,
			message: ({ label }) => `${label} must be greater than or equal to 0`,
		}),
});
export const powerProfileConfigSchema: yup.SchemaOf<PowerProfileConfig> = yup.object({
	name: yup.string().label('Name'),
	powerChances: yup.array(modifierChanceSchema).min(1).label('Power Chances'),
});
export const toolSurveySchema: yup.SchemaOf<ToolProfile> = yup.object({
	toolType: yup.mixed<ToolType>().oneOf(toolTypes).required().label('Tool Type'),
	toolRange: yup.mixed<ToolRange>().oneOf(toolRanges).required().label('Tool Range'),
	abilities: yup.array(abilitySchema).min(1).label('Abilities'),
	preferredDamageTypes: yup
		.array(yup.array(damageTypeSchema))
		.min(1, 'Must have at least one damage type')
		.label('Preferred Damage Types'),
	powerProfileConfigs: yup.array(powerProfileConfigSchema).min(1).label('Power Profile Configs'),
	possibleRestrictions: yup.array(yup.string().required()).min(1).label('Restrictions'),
});
export const classSurveySchema: yup.SchemaOf<ClassProfile> = yup.object({
	name: yup.string().required().label('Name'),
	role: yup.mixed<CharacterRole>().oneOf(roles).required().label('Role'),
	powerSource: yup.string().required().label('Power Source'),
	tools: yup.array(toolSurveySchema).min(1, 'Must have at least one tool'),
});
export const classSurveySchemaWithoutTools: yup.SchemaOf<Omit<ClassProfile, 'tools'>> = yup.object({
	name: yup.string().required().label('Name'),
	role: yup.mixed<CharacterRole>().oneOf(roles).required().label('Role'),
	powerSource: yup.string().required().label('Power Source'),
});
