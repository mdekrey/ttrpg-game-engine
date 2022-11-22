import { useId } from 'src/core/hooks/useId';
import { FieldValues, Path } from 'react-hook-form';
import { ComponentProps } from 'react';
import { Label } from '../label/label';
import { ValidationMessages } from '../ValidationMessages';
import { FieldProps } from '../FieldProps';
import { ControlledSelect, Select } from './select';

export const SelectField = ({
	label,
	className,
	id,
	...props
}: ComponentProps<typeof Select> & { label: string; className?: string }) => {
	const autoId = useId();
	return (
		<div className={className}>
			<Label htmlFor={id || autoId}>{label}</Label>
			<Select id={id || autoId} {...props} />
		</div>
	);
};

export const SelectFormField = <
	TFieldValues extends FieldValues = FieldValues,
	TName extends Path<TFieldValues> = Path<TFieldValues>
>({
	label,
	className,
	form,
	name,
	id,
	...props
}: FieldProps<ComponentProps<typeof Select>, 'control' | 'formState', TFieldValues, TName>) => {
	const autoId = useId();
	return (
		<div className={className}>
			<Label htmlFor={id || autoId}>{label}</Label>
			<ControlledSelect control={form.control} name={name} id={id || autoId} {...props} />
			<ValidationMessages errors={form.formState.errors} name={name} />
		</div>
	);
};
