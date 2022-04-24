import { createContext, Provider, useContext } from 'react';
import type { FieldValues, Path, UseFormReturn } from 'react-hook-form';

export type FieldContextInfo<
	TFieldValues extends FieldValues = FieldValues,
	TName extends Path<TFieldValues> = Path<TFieldValues>
> = {
	id: string;
	name: TName;
	form: UseFormReturn<TFieldValues>;
};

const FieldContext = createContext<FieldContextInfo | null>(null);

export const FieldProvider = FieldContext.Provider as Provider<FieldContextInfo>;
export function useFieldContext() {
	const context = useContext(FieldContext);
	if (!context) throw new Error('useFieldContext used without a field context provider');
	return context;
}
