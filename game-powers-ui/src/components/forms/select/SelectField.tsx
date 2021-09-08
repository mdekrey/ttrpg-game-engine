import { FieldValues, Path, PathValue } from 'react-hook-form';
import { FieldProps } from '../FieldProps';
import { ValidationMessages } from '../ValidationMessages';
import { ControlledSelectProps, ControlledSelect } from './select';

export function SelectField<
	TFieldValues extends FieldValues = FieldValues,
	TName extends Path<TFieldValues> = Path<TFieldValues>
>({
	className,
	form,
	name,
	...props
}: FieldProps<ControlledSelectProps<PathValue<TFieldValues, TName>>, 'control' | 'formState', TFieldValues, TName>) {
	return (
		<div className={className}>
			<ControlledSelect control={form.control} name={name} {...props} />
			<ValidationMessages errors={form.formState.errors} name={name} />
		</div>
	);
}
