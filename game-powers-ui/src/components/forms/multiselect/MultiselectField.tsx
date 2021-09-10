import { FieldValues, Path, PathValue } from 'react-hook-form';
import { FieldProps } from '../FieldProps';
import { ValidationMessages } from '../ValidationMessages';
import { ControlledMultiselectProps, ControlledMultiselect } from './multiselect';

export type MultiselectFieldProps<
	TFieldValues extends FieldValues = FieldValues,
	TName extends Path<TFieldValues> = Path<TFieldValues>
> = FieldProps<
	ControlledMultiselectProps<PathValue<TFieldValues, TName>[0]>,
	'control' | 'formState',
	TFieldValues,
	TName
>;

export function MultiselectField<
	TFieldValues extends FieldValues = FieldValues,
	TName extends Path<TFieldValues> = Path<TFieldValues>
>({ className, form, name, ...props }: MultiselectFieldProps<TFieldValues, TName>) {
	return (
		<div className={className}>
			<ControlledMultiselect control={form.control} name={name} {...props} />
			<ValidationMessages errors={form.formState.errors} name={name} />
		</div>
	);
}
