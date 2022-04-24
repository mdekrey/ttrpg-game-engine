import { ToolProfile } from 'api/models/ToolProfile';

export const defaultToolProfile: Readonly<ToolProfile> = {
	toolType: 'Weapon',
	toolRange: 'Melee',
	abilities: ['Strength'],
	preferredDamageTypes: [[]],
	powerProfileConfigs: [],
	possibleRestrictions: [
		'the target is bloodied',
		'you are dual wielding',
		'you are bloodied',
		'you have combat advantage against the target',
	],
};
