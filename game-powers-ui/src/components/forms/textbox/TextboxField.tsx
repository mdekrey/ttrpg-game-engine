import { useId } from 'core/hooks/useId';
import { ReactNode, ComponentProps } from 'react';
import { FieldValues, Path } from 'react-hook-form';
import { ControlledComponentProps } from '../Controlled';
import { Label } from '../label/label';
import { ValidationMessages } from '../ValidationMessages';
import { ControlledTextbox, Textbox } from './textbox';

export const TextboxField = <
	TFieldValues extends FieldValues = FieldValues,
	TName extends Path<TFieldValues> = Path<TFieldValues>
>({
	label,
	className,
	error,
	id,
	...props
}: ControlledComponentProps<ComponentProps<typeof Textbox>, TFieldValues, TName> & {
	label: ReactNode;
	className?: string;
	error: string | undefined;
}) => {
	const autoId = useId();
	return (
		<div className={className}>
			<Label htmlFor={id || autoId}>{label}</Label>
			<ControlledTextbox id={id || autoId} {...props} />
			<ValidationMessages message={error} />
		</div>
	);
};
