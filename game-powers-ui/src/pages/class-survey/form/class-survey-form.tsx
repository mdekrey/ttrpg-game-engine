/* eslint-disable react/no-array-index-key */
import { useMemo, useState } from 'react';
import { useGameForm } from 'core/hooks/useGameForm';
import { Button } from 'components/button/Button';
import { Card } from 'components/card/card';
import { SelectFormField, TextboxField } from 'components/forms';
import { ButtonRow } from 'components/ButtonRow';
import { ClassProfile } from 'api/models/ClassProfile';
import { classSurveySchemaWithoutTools, roles } from 'core/schemas/api';
import { YamlEditor } from 'components/monaco/YamlEditor';
import { SamplePowersSection } from './SamplePowersSection';
import { defaultToolProfile } from './defaultToolProfile';

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
	const { handleSubmit, ...form } = useGameForm<Omit<ClassProfile, 'tools' | 'locked'>>({
		defaultValues: defaultValues || {
			name: 'Custom Class',
			role: 'Controller',
			powerSource: 'Martial',
		},
		schema: classSurveySchemaWithoutTools,
	});

	const role = form.watch('role');
	const powerSource = form.watch('powerSource');
	const classProfile: ClassProfile = useMemo(
		() => ({ name: 'Unimportant', role, powerSource, tools, locked: false }),
		[role, powerSource, tools]
	);

	return (
		<form
			className={className}
			onSubmit={
				onSubmit &&
				handleSubmit((value) => {
					onSubmit({ ...value, tools, locked: false });
				})
			}>
			<Card className="grid grid-cols-6 gap-6">
				<TextboxField label="Class Name" className="col-span-6 sm:col-span-3" form={form} name="name" />
				<SelectFormField className="col-span-6 sm:col-span-3" label="Role" form={form} name="role">
					{roles.map((r) => (
						<option key={r} value={r}>
							{r}
						</option>
					))}
				</SelectFormField>
				<TextboxField label="PowerSource" className="col-span-6 sm:col-span-3" form={form} name="powerSource" />
				<div className="col-span-6 h-96">
					<YamlEditor value={tools} onChange={setTools} path="tools.yaml" />
				</div>
			</Card>
			<Card className="grid grid-cols-6 gap-6 mt-6">
				<SamplePowersSection classProfile={classProfile} onSaveTool={setTools} />
			</Card>
			<Card className="grid grid-cols-6 gap-6 mt-6">
				<ButtonRow className="col-span-6">
					<Button type="submit">Submit</Button>
				</ButtonRow>
			</Card>
		</form>
	);
}
