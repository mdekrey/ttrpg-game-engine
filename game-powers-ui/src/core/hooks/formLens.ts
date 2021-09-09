/* eslint-disable no-underscore-dangle */
import {
	KeepStateOptions,
	SetValueConfig,
	FieldPath,
	FieldPathValue,
	FieldValues,
	Path,
	UseFormReturn,
	UnpackNestedValue,
	FieldPathValues,
	RegisterOptions,
	Control,
	get,
} from 'react-hook-form';

type Mapped = 'getValues' | 'setValue' | 'control' | 'register' | 'unregister';
type Mappable<TFieldValues extends FieldValues, TContext extends object> = Pick<
	UseFormReturn<TFieldValues, TContext>,
	Mapped
>;

// TODO - this is a work in progress
export function lens<TFieldValues extends FieldValues, TContext extends object, TName extends Path<TFieldValues>>(
	form: Mappable<TFieldValues, TContext>,
	name: TName
): Mappable<FieldPathValue<TFieldValues, TName> & FieldValues, TContext> {
	type TField = FieldPathValue<TFieldValues, TName> & FieldValues;

	function getValues(): UnpackNestedValue<TField>;
	function getValues<TFieldName extends FieldPath<TField>>(inner: TFieldName): FieldPathValue<TField, TFieldName>;
	function getValues<TFieldNames extends FieldPath<TField>[]>(
		names: readonly [...TFieldNames]
	): [...FieldPathValues<TField, TFieldNames>];
	function getValues(inner?: Path<TField> | readonly Path<TField>[]) {
		if (inner === undefined) return form.getValues(name);
		if (typeof inner === 'string') return form.getValues(`${name}.${inner}` as Path<TFieldValues>);
		return form.getValues(inner.map((i) => `${name}.${i}` as Path<TFieldValues>));
	}

	function setValue<TFieldName extends FieldPath<TField> = FieldPath<TField>>(
		inner: TFieldName,
		value: UnpackNestedValue<FieldPathValue<TField, TFieldName>>,
		options?: SetValueConfig
	) {
		form.setValue<any>(`${name}.${inner}`, value, options);
	}

	function register<TFieldName extends FieldPath<TField>>(
		inner: TFieldName,
		options?: RegisterOptions<TField, TFieldName>
	) {
		return form.register(`${name}.${inner}` as any, options);
	}

	function unregister(
		inner?: FieldPath<TField> | FieldPath<TField>[] | readonly FieldPath<TField>[],
		options?: Omit<
			KeepStateOptions,
			'keepIsSubmitted' | 'keepSubmitCount' | 'keepValues' | 'keepDefaultValues' | 'keepErrors'
		> & {
			keepValue?: boolean;
			keepDefaultValue?: boolean;
			keepError?: boolean;
		}
	) {
		form.unregister(
			inner === undefined
				? name
				: typeof inner === 'string'
				? (`${name}.${inner}` as Path<TFieldValues>)
				: inner.map((i) => `${name}.${i}` as Path<TFieldValues>),
			options
		);
	}

	const control: Control<TField, TContext> = {
		...form.control,
		register,
		unregister,
		_subjects: null as any, // TODO
		_updateProps: null as any, // TODO
		_formState: null as any, // TODO
		_defaultValues: get(form.control._defaultValues, name),
	};

	return {
		getValues,
		setValue,
		register,
		unregister,
		control,
	};
}
