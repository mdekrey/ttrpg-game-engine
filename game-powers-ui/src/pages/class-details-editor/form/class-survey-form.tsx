/* eslint-disable react/no-array-index-key */
import { useState } from 'react';
import { useGameForm } from 'src/core/hooks/useGameForm';
import { Button } from 'src/components/button/Button';
import { Card } from 'src/components/card/card';
import { SelectFormField, TextboxField } from 'src/components/forms';
import { ButtonRow } from 'src/components/ButtonRow';
import { EditableClassDescriptor } from 'src/api/models/EditableClassDescriptor';
import { classSurveySchemaWithoutDescription, roles } from 'src/core/schemas/api';
import { ClassDescription } from 'src/components/mdx/ClassDescription';
import { MdxEditor } from 'src/components/monaco/MdxEditor';
import { MdxComponents } from 'src/components/layout/mdx-components';
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
	const { handleSubmit, ...form } = useGameForm<Omit<EditableClassDescriptor, 'description'>>({
		defaultValues: defaultValues || {
			name: 'Custom Class',
			role: 'Controller',
			powerSource: 'Martial',
			tools: [defaultToolProfile],
		},
		schema: classSurveySchemaWithoutDescription,
	});

	return (
		<form
			className={className}
			onSubmit={
				onSubmit &&
				handleSubmit((value) => {
					onSubmit({ ...value, description });
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
				<div className="col-span-6 h-96">{/* TODO: tool selection */}</div>
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
