/* eslint-disable react/no-array-index-key */
import { useState } from 'react';
import { useGameForm } from 'core/hooks/useGameForm';
import { Button } from 'components/button/Button';
import { Card } from 'components/card/card';
import { Select, SelectField, TextboxField } from 'components/forms';
import { ButtonRow } from 'components/ButtonRow';
import { ClassProfile } from 'api/models/ClassProfile';
import { ToolProfile } from 'api/models/ToolProfile';
import { classSurveySchemaWithoutTools, roles } from 'core/schemas/api';
import { YamlEditor } from 'components/monaco/YamlEditor';
import { SamplePowers } from './SamplePowers';

const defaultToolProfile: Readonly<ToolProfile> = {
	toolType: 'Weapon',
	toolRange: 'Melee',
	abilities: ['Strength'],
	preferredDamageTypes: ['Normal'],
	powerProfileConfigs: [
		{
			name: 'Any Power',
			powerChances: [{ selector: '$', weight: 1.0 }],
			modifierChances: [{ selector: '$', weight: 1.0 }],
		},
		{
			name: 'Accurate',
			powerChances: [
				{ selector: "$..[?(@.Name=='Non-Armor Defense' || @.Name=='To-Hit Bonus to Current Attack')]", weight: 1.0 },
			],
			modifierChances: [{ selector: '$', weight: 1.0 }],
		},
		{
			name: 'Follow-up Attack',
			powerChances: [
				{ selector: "$..[?(@.Name=='RequiredHitForNextAttack' || @.Name=='RequiresPreviousHit')]", weight: 1.0 },
			],
			modifierChances: [{ selector: '$', weight: 1.0 }],
		},
		{
			name: 'Two Attacks',
			powerChances: [{ selector: "$..[?(@.Name=='TwoHits')]", weight: 1.0 }],
			modifierChances: [{ selector: '$', weight: 1.0 }],
		},
		{
			name: 'Multiple Attacks',
			powerChances: [{ selector: "$..[?(@.Name=='UpToThreeTargets')]", weight: 1.0 }],
			modifierChances: [{ selector: '$', weight: 1.0 }],
		},
		{
			name: 'Multiattack (does this get triggered?)',
			powerChances: [{ selector: "$..[?(@.Name=='Multiattack')]", weight: 1.0 }],
			modifierChances: [{ selector: '$', weight: 1.0 }],
		},
		{
			name: 'Apply conditions',
			powerChances: [{ selector: "$..[?(@.Name=='Condition')]", weight: 1.0 }],
			modifierChances: [{ selector: '$', weight: 1.0 }],
		},
		{
			name: 'Reactions',
			powerChances: [{ selector: "$..[?(@.Name=='OpportunityAction')]", weight: 1.0 }],
			modifierChances: [{ selector: '$', weight: 1.0 }],
		},
		{
			name: 'Maneuver',
			powerChances: [{ selector: "$..[?(@.Name=='Skirmish Movement')]", weight: 1.0 }],
			modifierChances: [{ selector: '$', weight: 1.0 }],
		},
	],
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
	const [tools, setTools] = useState([defaultToolProfile]);
	const [selectedCfg, setSelectedCfg] = useState<null | { toolIndex: number; powerConfigIndex: number }>(null);
	const { handleSubmit, ...form } = useGameForm<Omit<ClassProfile, 'tools'>>({
		defaultValues: defaultValues || {
			name: 'Custom Class',
			role: 'Controller',
			powerSource: 'Martial',
		},
		schema: classSurveySchemaWithoutTools,
	});

	const classProfile: ClassProfile = { ...form.watch(), tools };

	return (
		<form
			className={className}
			onSubmit={
				onSubmit &&
				handleSubmit((value) => {
					onSubmit({ ...value, tools });
				})
			}>
			<Card className="grid grid-cols-6 gap-6">
				<TextboxField label="Class Name" className="col-span-6 sm:col-span-3" form={form} name="name" />
				<SelectField className="col-span-6 sm:col-span-3" label="Role" form={form} name="role" options={roles} />
				<TextboxField label="PowerSource" className="col-span-6 sm:col-span-3" form={form} name="powerSource" />
				<div className="col-span-6 h-96">
					<YamlEditor value={tools} onChange={setTools} path="tools.yaml" />
				</div>
				<div className="col-span-6">
					<Select
						value={selectedCfg}
						onChange={setSelectedCfg}
						label="Preview Powers"
						options={[
							null,
							...tools.flatMap((tool, toolIndex) =>
								tool.powerProfileConfigs.map((_, powerConfigIndex) => ({ toolIndex, powerConfigIndex }))
							),
						]}
						optionDisplay={(cfg) =>
							cfg === null
								? 'None'
								: `${tools[cfg.toolIndex].toolRange} ${tools[cfg.toolIndex].toolType}: ${
										tools[cfg.toolIndex].powerProfileConfigs[cfg.powerConfigIndex].name
								  }`
						}
						optionKey={(cfg) => (cfg === null ? '-' : `${cfg.toolIndex}-${cfg.powerConfigIndex}`)}
					/>
					{selectedCfg && (
						<SamplePowers
							classProfile={classProfile}
							toolIndex={selectedCfg.toolIndex}
							powerProfileIndex={selectedCfg.powerConfigIndex}
						/>
					)}
				</div>
				<ButtonRow className="col-span-6">
					<Button type="submit">Submit</Button>
				</ButtonRow>
			</Card>
		</form>
	);
}
