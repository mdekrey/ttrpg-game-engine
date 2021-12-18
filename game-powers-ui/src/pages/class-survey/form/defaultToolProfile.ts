import { ToolProfile } from 'api/models/ToolProfile';

export const defaultToolProfile: Readonly<ToolProfile> = {
	toolType: 'Weapon',
	toolRange: 'Melee',
	abilities: ['Strength'],
	preferredDamageTypes: [[]],
	powerProfileConfigs: [
		{
			name: 'Any Power',
			powerChances: [{ selector: '$', weight: 1.0 }],
		},
		{
			name: 'Accurate',
			powerChances: [
				{ selector: "$..[?(@.Name=='Non-Armor Defense' || @.Name=='To-Hit Bonus to Current Attack')]", weight: 1.0 },
			],
		},
		{
			name: 'Follow-up Attack',
			powerChances: [
				{ selector: "$..[?(@.Name=='RequiredHitForNextAttack' || @.Name=='RequiresPreviousHit')]", weight: 1.0 },
			],
		},
		{
			name: 'Two Attacks',
			powerChances: [{ selector: "$..[?(@.Name=='TwoHits')]", weight: 1.0 }],
		},
		{
			name: 'Multiple Attacks',
			powerChances: [{ selector: "$..[?(@.Name=='UpToThreeTargets')]", weight: 1.0 }],
		},
		{
			name: 'Apply conditions',
			powerChances: [{ selector: "$..[?(@.Name=='Condition')]", weight: 1.0 }],
		},
		{
			name: 'Reactions',
			powerChances: [{ selector: "$..[?(@.Name=='OpportunityAction')]", weight: 1.0 }],
		},
		{
			name: 'Maneuver',
			powerChances: [{ selector: "$..[?(@.Name=='Skirmish Movement')]", weight: 1.0 }],
		},
	],
	possibleRestrictions: [
		'the target is bloodied',
		'you are dual wielding',
		'you are bloodied',
		'you have combat advantage against the target',
	],
};
