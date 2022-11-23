import { DeepPartial, FieldValues, UnpackNestedValue, useForm, get, FieldPath, PathValue } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';

export function useGameForm<T extends FieldValues>({
	defaultValues,
	schema,
}: {
	defaultValues: UnpackNestedValue<T>;
	schema: yup.SchemaOf<T>;
}) {
	return useForm<T>({
		mode: 'onBlur',
		defaultValues: defaultValues as UnpackNestedValue<DeepPartial<T>>,
		resolver: yupResolver(schema),
	});
}

export const getPath = get as {
	<T extends FieldValues, TPath extends FieldPath<T>>(values: T, path: TPath): PathValue<T, TPath>;
};
