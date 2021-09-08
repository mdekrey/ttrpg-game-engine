import { ReactNode } from 'react';
import { ControllerRenderProps, FieldValues, Path, UseFormReturn } from 'react-hook-form';

export type FieldProps<
	T,
	TFieldValues extends FieldValues = FieldValues,
	TName extends Path<TFieldValues> = Path<TFieldValues>
> = {
	label: ReactNode;
	className?: string;
	form: Pick<UseFormReturn<TFieldValues>, 'control' | 'formState'>;
	// control: Control<TFieldValues, object>;
	name: TName;
	// error: string | undefined;
	defaultValue?: undefined;
} & Omit<T, keyof ControllerRenderProps | 'form'>;
