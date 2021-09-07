import { useForm } from 'react-hook-form';
import { useId } from 'core/hooks/useId';
import { Button } from 'components/button/Button';
import { Card } from 'components/card/card';
import { ControlledSelect, ControlledTextbox, Label } from 'components/forms';

type CharacterRole = 'Controller' | 'Defender' | 'Leader' | 'Striker';
type ToolType = 'Weapon' | 'Implement';

type ClassSurveyForm = {
	name: string;
	role: CharacterRole;
	toolType: ToolType;
};

const roles: CharacterRole[] = ['Controller', 'Defender', 'Leader', 'Striker'];
const toolTypes: ToolType[] = ['Weapon', 'Implement'];

export function ClassSurvey({
	className,
	onSubmit,
}: {
	className?: string;
	onSubmit?: (form: ClassSurveyForm) => void;
}) {
	const { handleSubmit, control } = useForm<ClassSurveyForm>({
		defaultValues: {
			name: 'Custom Class',
			role: 'Controller',
			toolType: 'Weapon',
		},
	});
	const id = useId();

	return (
		<form className={className} onSubmit={onSubmit && handleSubmit(onSubmit)}>
			<Card>
				<div className="grid grid-cols-6 gap-6">
					<div className="col-span-6 sm:col-span-3">
						<Label htmlFor={`class-name-${id}`}>Name</Label>
						<ControlledTextbox control={control} name="name" id={`class-name-${id}`} />
					</div>
					<div className="col-span-6 sm:col-span-3">
						<ControlledSelect
							control={control}
							name="role"
							options={roles}
							optionKey={(opt) => opt}
							optionDisplay={(opt) => opt}
						/>
					</div>
					<div className="col-span-6">
						<ControlledSelect
							control={control}
							name="toolType"
							options={toolTypes}
							optionKey={(opt) => opt}
							optionDisplay={(opt) => opt}
						/>
					</div>
				</div>
			</Card>
			<Card className="mt-4">
				<Button type="submit">Submit</Button>
			</Card>
		</form>
	);
}
