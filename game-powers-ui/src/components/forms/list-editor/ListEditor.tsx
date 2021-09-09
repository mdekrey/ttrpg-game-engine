import { Button } from 'components/button/Button';
import { ButtonRow } from 'components/ButtonRow';
import { Card } from 'components/card/card';
import { ReactNode, useState } from 'react';
import { FieldValues, Path, PathValue } from 'react-hook-form';
import { FieldPropsBase } from '../FieldProps';
import { ValidationMessages } from '../ValidationMessages';

export type ListFieldProps<
	TFieldValues,
	TName extends Path<TFieldValues>,
	TItemName extends Path<TFieldValues> & `${TName}.${number}`
> = {
	className?: string;
	depth?: number;
	itemEditor: (path: `${TName}.${number}`) => ReactNode;
	defaultItem: PathValue<TFieldValues, TItemName>;
	addLabel: ReactNode;
	removeLabel: ReactNode;
	label: ReactNode;
} & FieldPropsBase<'formState' | 'getValues' | 'setValue', TFieldValues, TName>;

export function ListField<
	TFieldValues extends FieldValues,
	TName extends Path<TFieldValues>,
	TItemName extends Path<TFieldValues> & `${TName}.${number}`
>({
	className,
	depth = 0,
	itemEditor,
	form: { formState, getValues, setValue },
	name,
	defaultItem: defaultValue,
	addLabel,
	removeLabel,
	label,
}: ListFieldProps<TFieldValues, TName, TItemName>) {
	const [itemKeys, setItemKeys] = useState([0]);

	function addItem() {
		setItemKeys((t) => [...t, t.length]);
		setValue(name, [...getValues(name), defaultValue] as PathValue<TFieldValues, TName>);
	}

	function remove(index: number) {
		setItemKeys((t) => [...t.slice(0, index), ...t.slice(index + 1)]);
		const current = getValues(name);
		setValue(name, [...current.slice(0, index), ...current.slice(index + 1)] as PathValue<TFieldValues, TName>);
	}

	return (
		<div className={className}>
			<ButtonRow>
				<Button type="button" onClick={addItem}>
					{addLabel}
				</Button>
				<span className="block text-sm font-medium text-gray-700 flex-grow">{label}</span>
			</ButtonRow>
			{itemKeys.map((itemKey, index) => (
				<Card key={itemKey} className="mt-4 grid grid-cols-1 gap-6" depth={depth + 1}>
					{itemEditor(`${name}.${index}` as `${TName}.${number}` & Path<TFieldValues>)}
					<ButtonRow>
						<Button type="button" onClick={() => remove(index)}>
							{removeLabel}
						</Button>
					</ButtonRow>
				</Card>
			))}
			<ValidationMessages errors={formState.errors} name={name} />
		</div>
	);
}
