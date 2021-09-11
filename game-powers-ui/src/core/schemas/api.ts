import * as yup from 'yup';
import {
	Ability,
	CharacterRole,
	ClassProfile,
	DamageType,
	ModifierChance,
	PowerProfileConfig,
	ToolProfile,
	ToolRange,
	ToolType,
} from 'api';

export const roles: CharacterRole[] = [
	CharacterRole.Controller,
	CharacterRole.Defender,
	CharacterRole.Leader,
	CharacterRole.Striker,
];
export const toolTypes: ToolType[] = [ToolType.Weapon, ToolType.Implement];
export const toolRanges: ToolRange[] = [ToolRange.Melee, ToolRange.Range];
export const abilities: Ability[] = [
	Ability.Strength,
	Ability.Constitution,
	Ability.Dexterity,
	Ability.Intelligence,
	Ability.Wisdom,
	Ability.Charisma,
];
export const damageTypes: DamageType[] = [
	DamageType.Normal,
	DamageType.Fire,
	DamageType.Cold,
	DamageType.Necrotic,
	DamageType.Radiant,
	DamageType.Lightning,
	DamageType.Thunder,
	DamageType.Poison,
	DamageType.Force,
];

export const abilitySchema = yup.mixed().oneOf(abilities).required().defined() as yup.SchemaOf<Ability>;
export const damageTypeSchema = yup.string().oneOf(damageTypes).required().defined() as yup.SchemaOf<DamageType>;
export const modifierChanceSchema: yup.SchemaOf<ModifierChance> = yup.object({
	selector: yup.string().required(),
	weight: yup.number().required().positive(),
});
export const powerProfileConfigSchema: yup.SchemaOf<PowerProfileConfig> = yup.object({
	modifierChances: yup.array(modifierChanceSchema).min(1).label('Modifier Chances'),
	powerChances: yup.array(modifierChanceSchema).min(1).label('Power Chances'),
});
export const toolSurveySchema: yup.SchemaOf<ToolProfile> = yup.object({
	toolType: yup.mixed<ToolType>().oneOf(toolTypes).required().label('Tool Type'),
	toolRange: yup.mixed<ToolRange>().oneOf(toolRanges).required().label('Tool Range'),
	abilities: yup.array(abilitySchema).min(1).label('Abilities'),
	preferredDamageTypes: yup
		.array(damageTypeSchema)
		.min(1, 'Must have at least one damage type')
		.label('Preferred Damage Types'),
	powerProfileConfig: powerProfileConfigSchema,
});
export const classSurveySchema: yup.SchemaOf<ClassProfile> = yup.object({
	name: yup.string().required().label('Name'),
	role: yup.mixed<CharacterRole>().oneOf(roles).required().label('Role'),
	powerSource: yup.string().required().label('Power Source'),
	tools: yup.array(toolSurveySchema).min(1, 'Must have at least one tool'),
});
