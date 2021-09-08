import { ReactNode } from 'react';
import { ControllerRenderProps, FieldValues, Path, UseFormReturn } from 'react-hook-form';

export type FieldPropsBase<
	TNeededFormParts extends keyof UseFormReturn,
	TFieldValues extends FieldValues = FieldValues,
	TName extends Path<TFieldValues> = Path<TFieldValues>
> = {
	label: ReactNode;
	className?: string;
	form: Pick<UseFormReturn<TFieldValues>, TNeededFormParts>;
	name: TName;
	defaultValue?: undefined;
};

export type FieldProps<
	T,
	TNeededFormParts extends keyof UseFormReturn,
	TFieldValues extends FieldValues = FieldValues,
	TName extends Path<TFieldValues> = Path<TFieldValues>
> = FieldPropsBase<TNeededFormParts, TFieldValues, TName> & Omit<T, keyof ControllerRenderProps | 'form'>;
