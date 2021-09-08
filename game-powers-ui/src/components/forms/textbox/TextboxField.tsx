import { useId } from 'core/hooks/useId';
import { FieldValues, Path } from 'react-hook-form';
import { ComponentProps } from 'react';
import { Label } from '../label/label';
import { ValidationMessages } from '../ValidationMessages';
import { FieldProps } from '../FieldProps';
import { ControlledTextbox, Textbox } from './textbox';

export const TextboxField = <
	TFieldValues extends FieldValues = FieldValues,
	TName extends Path<TFieldValues> = Path<TFieldValues>
>({
	label,
	className,
	form,
	name,
	id,
	...props
}: FieldProps<ComponentProps<typeof Textbox>, 'control' | 'formState', TFieldValues, TName>) => {
	const autoId = useId();
	return (
		<div className={className}>
			<Label htmlFor={id || autoId}>{label}</Label>
			<ControlledTextbox control={form.control} name={name} id={id || autoId} {...props} />
			<ValidationMessages errors={form.formState.errors} name={name} />
		</div>
	);
};
