import { Listbox, Transition } from '@headlessui/react';
import { CheckIcon, SelectorIcon, XIcon } from '@heroicons/react/solid';
import classNames from 'classnames';
import { ReactNode, Fragment, forwardRef, ForwardedRef } from 'react';
import { FieldValues, Path, PathValue, UseControllerProps } from 'react-hook-form';
import { Controlled } from '../Controlled';

export type ControlledMultiselectProps<T> = {
	label: ReactNode;
	options: readonly T[];
	optionKey?: (opt: Readonly<T>) => string;
	optionHeaderDisplay?: (opt: Readonly<T>) => ReactNode;
	optionDisplay?: (opt: Readonly<T>) => ReactNode;
	disabled?: boolean;
};

export type MultiselectProps<T> = ControlledMultiselectProps<T> & {
	value: readonly T[];
	onChange: (value: readonly T[]) => void;

	name?: string;
	onBlur?: () => void;
};

function MultiselectComponent<T>(
	{
		onChange,
		label,
		value,
		options,
		optionKey,
		optionHeaderDisplay,
		optionDisplay,
		disabled,
		name,
		onBlur,
		...props
	}: MultiselectProps<T>,
	ref: ForwardedRef<HTMLButtonElement>
) {
	const toKey = optionKey || ((v: T) => v as unknown as string);
	const toDisplay = optionDisplay || ((v: T) => v as unknown as ReactNode);
	const toHeaderDisplay = optionHeaderDisplay || toDisplay;

	function toggle(toToggle: T) {
		onChange(value.includes(toToggle) ? value.filter((v) => v !== toToggle) : [...value, toToggle]);
		if (onBlur) onBlur();
	}

	return (
		<Listbox value={null} onChange={toggle} {...props} disabled={disabled}>
			<Listbox.Label className="block text-sm font-medium text-black">{label}</Listbox.Label>
			<div className="relative">
				<Listbox.Button
					className={classNames(
						{ 'border-gray-300': !disabled, 'border-gray-50': disabled },
						'mt-1 py-1 px-1',
						'block w-full shadow-sm sm:text-sm',
						'border rounded-md',
						'focus:ring focus:ring-blue-dark focus:border-blue-dark outline-none transition-shadow',
						'text-left relative'
					)}
					onBlur={onBlur}
					name={name}
					ref={ref}>
					<span className="block truncate">
						{value.map((option) => (
							<button
								className="px-2 py-1 bg-blue-100 rounded text-blue-700 inline-flex items-center mr-1 text-sm"
								type="button"
								onClick={(ev) => {
									ev.stopPropagation();
									toggle(option);
								}}
								key={toKey(option)}>
								{toHeaderDisplay(option)}
								<XIcon className="w-5 h-5" aria-hidden="true" />
							</button>
						))}
						<span className="py-1 inline-block">&nbsp;</span>
					</span>
					<span className="absolute inset-y-0 right-0 flex items-center pr-2 pointer-events-none">
						<SelectorIcon className="w-5 h-5 text-gray-400" aria-hidden="true" />
					</span>
				</Listbox.Button>
				<Transition as={Fragment} leave="transition ease-in duration-100" leaveFrom="opacity-100" leaveTo="opacity-0">
					<Listbox.Options className="absolute w-full py-1 mt-1 overflow-auto text-base bg-white rounded-md shadow-lg max-h-60 ring-1 ring-black ring-opacity-5 focus:outline-none sm:text-sm z-10">
						{options.map((option) => (
							<Listbox.Option
								key={toKey(option)}
								value={option}
								onBlur={onBlur}
								className={({ active }) =>
									classNames(
										{
											'text-blue-900 bg-blue-100': active,
											'text-gray-900': !active,
										},
										`cursor-default select-none relative py-2 pl-10 pr-4`
									)
								}>
								{({ active }) => {
									const selected = value.includes(option);
									return (
										<>
											<span className={`${selected ? 'font-medium' : 'font-normal'} block truncate`}>
												{toDisplay(option)}
											</span>
											{selected ? (
												<span
													className={`${
														active ? 'text-blue-600' : 'text-blue-600'
													} absolute inset-y-0 left-0 flex items-center pl-3`}>
													<CheckIcon className="w-5 h-5" aria-hidden="true" />
												</span>
											) : null}
										</>
									);
								}}
							</Listbox.Option>
						))}
					</Listbox.Options>
				</Transition>
			</div>
		</Listbox>
	);
}

export const Multiselect = forwardRef<HTMLButtonElement, MultiselectProps<unknown>>(MultiselectComponent) as <T>(
	props: MultiselectProps<T>
) => JSX.Element;

export const ControlledMultiselect = Controlled(Multiselect) as <
	TFieldValues extends FieldValues = FieldValues,
	TName extends Path<TFieldValues> = Path<TFieldValues>
>(
	props: ControlledMultiselectProps<PathValue<TFieldValues, TName>> & UseControllerProps<TFieldValues, TName>
) => JSX.Element;
