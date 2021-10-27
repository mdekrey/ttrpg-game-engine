import { useState } from 'react';
import { useGameForm } from 'core/hooks/useGameForm';
import { Button } from 'components/button/Button';
import { Card } from 'components/card/card';
import { SelectField, TextboxField } from 'components/forms';
import { ButtonRow } from 'components/ButtonRow';
import { ClassProfile } from 'api/models/ClassProfile';
import { ToolProfile } from 'api/models/ToolProfile';
import { classSurveySchemaWithoutTools, roles } from 'core/schemas/api';
import { YamlEditor } from 'components/monaco/YamlEditor';

const defaultToolProfile: Readonly<ToolProfile> = {
	toolType: 'Weapon',
	toolRange: 'Melee',
	abilities: ['Strength'],
	preferredDamageTypes: ['Normal'],
	powerProfileConfig: {
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
		modifierChances: [{ selector: '$', weight: 1.0 }],
	},
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
	const { handleSubmit, ...form } = useGameForm<Omit<ClassProfile, 'tools'>>({
		defaultValues: defaultValues || {
			name: 'Custom Class',
			role: 'Controller',
			powerSource: 'Martial',
		},
		schema: classSurveySchemaWithoutTools,
	});

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
				<ButtonRow className="col-span-6">
					<Button type="submit">Submit</Button>
				</ButtonRow>
			</Card>
		</form>
	);
}
