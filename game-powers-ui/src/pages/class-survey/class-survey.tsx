import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import { useId } from 'core/hooks/useId';
import { Button } from 'components/button/Button';
import { Card } from 'components/card/card';
import { ControlledSelect, ControlledTextbox, Label, ValidationMessages } from 'components/forms';

type CharacterRole = 'Controller' | 'Defender' | 'Leader' | 'Striker';
type ToolType = 'Weapon' | 'Implement';

type ClassSurveyForm = {
	name: string;
	role: CharacterRole;
	toolType: ToolType;
};

const roles: CharacterRole[] = ['Controller', 'Defender', 'Leader', 'Striker'];
const toolTypes: ToolType[] = ['Weapon', 'Implement'];
const schema: yup.SchemaOf<ClassSurveyForm> = yup.object({
	name: yup.string().required().label('Name'),
	role: yup.mixed<CharacterRole>().oneOf(roles).required().label('Role'),
	toolType: yup.mixed<ToolType>().oneOf(toolTypes).required().label('Tool Type'),
});

export function ClassSurvey({
	className,
	onSubmit,
}: {
	className?: string;
	onSubmit?: (form: ClassSurveyForm) => void;
}) {
	const {
		handleSubmit,
		control,
		formState: { errors },
	} = useForm<ClassSurveyForm>({
		mode: 'onBlur',
		defaultValues: {
			name: 'Custom Class',
			role: 'Controller',
			toolType: 'Weapon',
		},
		resolver: yupResolver(schema),
	});
	const id = useId();

	return (
		<form className={className} onSubmit={onSubmit && handleSubmit(onSubmit)}>
			<Card>
				<div className="grid grid-cols-6 gap-6">
					<div className="col-span-6 sm:col-span-3">
						<Label htmlFor={`class-name-${id}`}>Name</Label>
						<ControlledTextbox control={control} name="name" id={`class-name-${id}`} />
						<ValidationMessages message={errors.name?.message} />
					</div>
					<div className="col-span-6 sm:col-span-3">
						<ControlledSelect
							control={control}
							name="role"
							options={roles}
							optionKey={(opt) => opt}
							optionDisplay={(opt) => opt}
						/>
						<ValidationMessages message={errors.role?.message} />
					</div>
					<div className="col-span-6">
						<ControlledSelect
							control={control}
							name="toolType"
							options={toolTypes}
							optionKey={(opt) => opt}
							optionDisplay={(opt) => opt}
						/>
						<ValidationMessages message={errors.toolType?.message} />
					</div>
				</div>
			</Card>
			<Card className="mt-4">
				<Button type="submit">Submit</Button>
			</Card>
		</form>
	);
}
