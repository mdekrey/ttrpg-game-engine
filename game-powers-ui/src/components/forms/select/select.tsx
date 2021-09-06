import { Listbox, Transition } from '@headlessui/react';
import { CheckIcon, SelectorIcon } from '@heroicons/react/solid';
import classNames from 'classnames';
import { ReactNode, Fragment } from 'react';
import { merge } from 'core/jsx/merge';
import { disabledInputBorder, inputBorder } from '../templates';

export type SelectProps<T> = {
	value: T;
	options: T[];
	optionKey: (opt: T) => string;
	optionHeaderDisplay?: (opt: T) => ReactNode;
	optionDisplay: (opt: T) => ReactNode;
	disabled?: boolean;
	onChange: (value: T) => void;
};

export function Select<T>({
	value,
	options,
	optionKey,
	optionHeaderDisplay,
	optionDisplay,
	disabled,
	...props
}: SelectProps<T>) {
	return (
		<Listbox value={value} {...props} disabled={disabled}>
			<Listbox.Label className="block text-sm font-medium text-black">Tool</Listbox.Label>
			<div className="relative">
				{merge(
					disabled ? disabledInputBorder : inputBorder,
					<Listbox.Button className="text-left relative">
						<span className="block truncate">{(optionHeaderDisplay || optionDisplay)(value)}</span>
						<span className="absolute inset-y-0 right-0 flex items-center pr-2 pointer-events-none">
							<SelectorIcon className="w-5 h-5 text-gray-400" aria-hidden="true" />
						</span>
					</Listbox.Button>
				)}
				<Transition as={Fragment} leave="transition ease-in duration-100" leaveFrom="opacity-100" leaveTo="opacity-0">
					<Listbox.Options className="absolute w-full py-1 mt-1 overflow-auto text-base bg-white rounded-md shadow-lg max-h-60 ring-1 ring-black ring-opacity-5 focus:outline-none sm:text-sm">
						{options.map((option) => (
							<Listbox.Option
								key={optionKey(option)}
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
											{optionDisplay(option)}
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
