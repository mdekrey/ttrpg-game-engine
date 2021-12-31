/* eslint-disable react/no-array-index-key */
import { useMemo, useState } from 'react';
import { useGameForm } from 'core/hooks/useGameForm';
import { Button } from 'components/button/Button';
import { Card } from 'components/card/card';
import { SelectFormField, TextboxField } from 'components/forms';
import { ButtonRow } from 'components/ButtonRow';
import { EditableClassDescriptor } from 'api/models/EditableClassDescriptor';
import { classSurveySchemaWithoutEditors, roles } from 'core/schemas/api';
import { ClassDescription } from 'components/mdx/ClassDescription';
import { YamlEditor } from 'components/monaco/YamlEditor';
import { MdxEditor } from 'components/monaco/MdxEditor';
import { MdxComponents } from 'components/layout/mdx-components';
import { SamplePowersSection } from './SamplePowersSection';
import { defaultToolProfile } from './defaultToolProfile';

export function ClassSurveyForm({
	className,
	onSubmit,
	onCancel,
	defaultValues,
}: {
	className?: string;
	onSubmit?: (form: EditableClassDescriptor) => void;
	onCancel?: () => void;
	defaultValues?: EditableClassDescriptor;
}) {
	const [description, setDescription] = useState(defaultValues?.description || '');
	const [tools, setTools] = useState(defaultValues?.tools || [defaultToolProfile]);
	const { handleSubmit, ...form } = useGameForm<Omit<EditableClassDescriptor, 'tools' | 'description'>>({
		defaultValues: defaultValues || {
			name: 'Custom Class',
			role: 'Controller',
			powerSource: 'Martial',
		},
		schema: classSurveySchemaWithoutEditors,
	});

	const role = form.watch('role');
	const powerSource = form.watch('powerSource');
	const classProfile: EditableClassDescriptor = useMemo(
		() => ({ name: 'Unimportant', description: '', role, powerSource, tools }),
		[role, powerSource, tools]
	);

	return (
		<form
			className={className}
			onSubmit={
				onSubmit &&
				handleSubmit((value) => {
					onSubmit({ ...value, tools, description });
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
				<div className="col-span-3 h-96 col-start-1">
					<MdxEditor value={description} onChange={setDescription} />
				</div>
				<div className="col-span-3 h-96">
					<MdxComponents>
						<ClassDescription mdx={description || ''} />
					</MdxComponents>
				</div>
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
					{onCancel && (
						<Button type="button" onClick={onCancel} look="cancel">
							Cancel
						</Button>
					)}
				</ButtonRow>
			</Card>
		</form>
	);
}
