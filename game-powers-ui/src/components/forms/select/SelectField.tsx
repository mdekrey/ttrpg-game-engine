import { FieldValues, Path, PathValue, UseControllerProps } from 'react-hook-form';
import { ValidationMessages } from '../ValidationMessages';
import { ControlledSelectProps, ControlledSelect } from './select';

export function SelectField<
	TFieldValues extends FieldValues = FieldValues,
	TName extends Path<TFieldValues> = Path<TFieldValues>
>({
	className,
	error,
	...props
}: ControlledSelectProps<PathValue<TFieldValues, TName>> &
	UseControllerProps<TFieldValues, TName> & { error: string | undefined; className?: string }) {
	return (
		<div className={className}>
			<ControlledSelect {...props} />
			<ValidationMessages message={error} />
		</div>
	);
}
