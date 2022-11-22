import { ReactNode, useMemo } from 'react';
import { FieldValues, Path, UseFormReturn } from 'react-hook-form';
import { useId } from 'src/core/hooks/useId';
import { FieldContextInfo, FieldProvider } from './field.context';

export type FieldProps<
	TFieldValues extends FieldValues = FieldValues,
	TName extends Path<TFieldValues> = Path<TFieldValues>
> = {
	id?: string;
	name: TName;
	className?: string;
	form: UseFormReturn<TFieldValues>;
	children?: ReactNode;
};

export const Field = <
	TFieldValues extends FieldValues = FieldValues,
	TName extends Path<TFieldValues> = Path<TFieldValues>
>({
	className,
	form,
	name,
	id,
	children,
}: FieldProps<TFieldValues, TName>) => {
	const autoId = useId();
	const provided = useMemo(
		(): FieldContextInfo<TFieldValues, TName> => ({
			id: id ?? autoId,
			form,
			name,
		}),
		[autoId, id, name, form]
	);
	return (
		<div className={className}>
			<FieldProvider value={provided as FieldContextInfo}>{children}</FieldProvider>
		</div>
	);
};
