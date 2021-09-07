import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import { Button } from 'components/button/Button';
import { Card } from 'components/card/card';
import { SelectField, TextboxField } from 'components/forms';
import { useState } from 'react';

type CharacterRole = 'Controller' | 'Defender' | 'Leader' | 'Striker';
type ToolType = 'Weapon' | 'Implement';

type ToolSurveyForm = {
	toolType: ToolType;
	name: string;
};

type ClassSurveyForm = {
	name: string;
	role: CharacterRole;
	tools: ToolSurveyForm[];
};

const roles: CharacterRole[] = ['Controller', 'Defender', 'Leader', 'Striker'];
const toolTypes: ToolType[] = ['Weapon', 'Implement'];

const toolSurveySchema: yup.SchemaOf<ToolSurveyForm> = yup.object({
	toolType: yup.mixed<ToolType>().oneOf(toolTypes).required().label('Tool Type'),
	name: yup.string().required().label('Name'),
});
const classSurveySchema: yup.SchemaOf<ClassSurveyForm> = yup.object({
	name: yup.string().required().label('Name'),
	role: yup.mixed<CharacterRole>().oneOf(roles).required().label('Role'),
	tools: yup.array(toolSurveySchema),
});

export function ClassSurvey({
	className,
	onSubmit,
}: {
	className?: string;
	onSubmit?: (form: ClassSurveyForm) => void;
}) {
	const [tools, setTools] = useState([0]);
	const {
		handleSubmit,
		control,
		formState: { errors },
		setValue,
		getValues,
	} = useForm<ClassSurveyForm>({
		mode: 'onBlur',
		defaultValues: {
			name: 'Custom Class',
			role: 'Controller',
			tools: [{ toolType: 'Weapon', name: '' }],
		},
		resolver: yupResolver(classSurveySchema),
	});

	return (
		<form className={className} onSubmit={onSubmit && handleSubmit(onSubmit)}>
			<Card className="grid grid-cols-6 gap-6">
				<TextboxField
					label="Class Name"
					className="col-span-6 sm:col-span-3"
					control={control}
					name="name"
					error={errors.name?.message}
				/>
				<SelectField
					className="col-span-6 sm:col-span-3"
					label="Role"
					control={control}
					name="role"
					options={roles}
					optionKey={(opt) => opt}
					optionDisplay={(opt) => opt}
					error={errors.role?.message}
				/>
			</Card>
			{tools.map((toolKey, index) => (
				<Card key={toolKey} className="mt-4 grid grid-cols-6 gap-6">
					<SelectField
						className="col-span-6 sm:col-span-3"
						label="Tool"
						control={control}
						name={`tools.${index}.toolType`}
						options={toolTypes}
						optionKey={(opt) => opt}
						optionDisplay={(opt) => opt}
						error={errors.tools && errors.tools[index]?.toolType?.message}
					/>
					<TextboxField
						label="Tool Name"
						className="col-span-6 sm:col-span-3"
						control={control}
						name={`tools.${index}.name`}
						error={errors.tools && errors.tools[index]?.name?.message}
					/>
					<Button type="button" onClick={() => removeTool(index)}>
						Remove Tool
					</Button>
				</Card>
			))}
			<Card className="mt-4 flex flex-row-reverse justify-start gap-4">
				<Button type="submit">Submit</Button>
				<Button type="button" onClick={addTool}>
					Add Tool
				</Button>
			</Card>
		</form>
	);

	function addTool() {
		setTools((t) => [...t, t.length]);
		setValue('tools', [...getValues('tools'), { toolType: 'Weapon', name: '' }]);
	}

	function removeTool(index: number) {
		setTools((t) => [...t.slice(0, index), ...t.slice(index + 1)]);
		const currentTools = getValues('tools');
		setValue('tools', [...currentTools.slice(0, index), ...currentTools.slice(index + 1)]);
	}
}
