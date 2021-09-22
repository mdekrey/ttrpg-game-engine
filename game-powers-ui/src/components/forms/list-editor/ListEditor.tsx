import { PlusIcon } from '@heroicons/react/solid';
import { Button } from 'components/button/Button';
import { ButtonRow } from 'components/ButtonRow';
import { Fragment, ReactNode } from 'react';
import { FieldValues, Path, PathValue } from 'react-hook-form';
import { FieldPropsBase } from '../FieldProps';
import { ValidationMessages } from '../ValidationMessages';

export type ListFieldProps<
	TFieldValues,
	TName extends Path<TFieldValues>,
	TItemName extends Path<TFieldValues> & `${TName}.${number}`
> = {
	className?: string;
	itemEditor: (path: `${TName}.${number}`, onRemove: () => void) => ReactNode;
	defaultNewItem: () => PathValue<TFieldValues, TItemName>;
	addLabel: string;
	label: ReactNode;
} & FieldPropsBase<'formState' | 'getValues' | 'setValue' | 'watch', TFieldValues, TName>;

export function ListField<
	TFieldValues extends FieldValues,
	TName extends Path<TFieldValues>,
	TItemName extends Path<TFieldValues> & `${TName}.${number}`
>({
	className,
	itemEditor,
	form: { formState, getValues, setValue, watch },
	name,
	defaultNewItem: defaultValue,
	addLabel,
	label,
}: ListFieldProps<TFieldValues, TName, TItemName>) {
	const itemKeys = Array((watch(name) || []).length)
		.fill(0)
		.map((_, i) => i);

	function addItem() {
		setValue(name, [...getValues(name), defaultValue()] as PathValue<TFieldValues, TName>);
	}

	function remove(index: number) {
		const current = getValues(name);
		setValue(name, [...current.slice(0, index), ...current.slice(index + 1)] as PathValue<TFieldValues, TName>);
	}

	return (
		<div className={className}>
			<ButtonRow>
				<Button type="button" contents="icon" onClick={addItem} title={addLabel}>
					<PlusIcon className="h-em w-em" />
				</Button>
				<span className="block text-sm font-medium text-gray-700 flex-grow">{label}</span>
			</ButtonRow>
			{itemKeys.map((itemKey, index) => (
				<Fragment key={itemKey}>
					{itemEditor(`${name}.${index}` as `${TName}.${number}` & Path<TFieldValues>, () => remove(index))}
				</Fragment>
			))}
			<ValidationMessages errors={formState.errors} name={name} />
		</div>
	);
}
