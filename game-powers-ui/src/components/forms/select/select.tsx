import { Listbox, Transition } from '@headlessui/react';
import { CheckIcon, SelectorIcon } from '@heroicons/react/solid';
import classNames from 'classnames';
import { ReactNode, Fragment, forwardRef, ForwardedRef } from 'react';
import { pipeJsx } from 'core/jsx/pipeJsx';
import { FieldValues, Path, PathValue, UseControllerProps } from 'react-hook-form';
import { inputBorder, label as labelTemplate } from '../templates';
import { Controlled } from '../Controlled';

export type ControlledSelectProps<T> = {
	label: ReactNode;
	options: readonly T[];
	optionKey?: (opt: Readonly<T>) => string;
	optionHeaderDisplay?: (opt: Readonly<T>) => ReactNode;
	optionDisplay?: (opt: Readonly<T>) => ReactNode;
	disabled?: boolean;
};

export type SelectProps<T> = ControlledSelectProps<T> & {
	value: T;
	onChange: (value: T) => void;

	name?: string;
	onBlur?: () => void;
};

function SelectComponent<T>(
	{
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
	}: SelectProps<T>,
	ref: ForwardedRef<HTMLButtonElement>
) {
	const toKey = optionKey || ((v: T) => v as unknown as string);
	const toDisplay = optionDisplay || ((v: T) => v as unknown as ReactNode);
	const toHeaderDisplay = optionHeaderDisplay || toDisplay;

	return (
		<Listbox value={value} {...props} disabled={disabled}>
			{pipeJsx(<Listbox.Label>{label}</Listbox.Label>, labelTemplate())}
			<div className="relative">
				{pipeJsx(
					<Listbox.Button className="text-left relative" onBlur={onBlur} name={name} ref={ref}>
						<span className="block truncate">{toHeaderDisplay(value)}</span>
						<span className="absolute inset-y-0 right-0 flex items-center pr-px pointer-events-none">
							<SelectorIcon className="w-5 h-5 text-gray-400" aria-hidden="true" />
						</span>
					</Listbox.Button>,
					inputBorder(disabled || false)
				)}
				<Transition as={Fragment} leave="transition ease-in duration-100" leaveFrom="opacity-100" leaveTo="opacity-0">
					<Listbox.Options className="absolute w-full py-1 mt-1 overflow-auto text-base bg-white rounded-md shadow-lg max-h-60 ring-1 ring-black ring-opacity-5 focus:outline-none sm:text-sm z-10">
						{options.map((option) => (
							<Listbox.Option
								key={toKey(option)}
								value={option}
								className={({ active }) =>
									classNames(
										{
											'text-blue-900 bg-blue-100': active,
											'text-gray-900': !active,
										},
										`cursor-default select-none relative py-2 pl-10 pr-4`
									)
								}>
								{({ selected, active }) => (
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
								)}
							</Listbox.Option>
						))}
					</Listbox.Options>
				</Transition>
			</div>
		</Listbox>
	);
}

export const Select = forwardRef<HTMLButtonElement, SelectProps<unknown>>(SelectComponent) as <T>(
	props: SelectProps<T>
) => JSX.Element;

export const ControlledSelect = Controlled(Select) as <
	TFieldValues extends FieldValues = FieldValues,
	TName extends Path<TFieldValues> = Path<TFieldValues>
>(
	props: ControlledSelectProps<PathValue<TFieldValues, TName>> & UseControllerProps<TFieldValues, TName>
) => JSX.Element;
