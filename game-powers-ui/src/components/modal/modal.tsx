import { Dialog, Transition } from '@headlessui/react';
import { mergeStyles } from 'core/jsx/mergeStyles';
import { pipeJsx } from 'core/jsx/pipeJsx';
import { Fragment } from 'react';

const sizes = {
	full: <div className="w-full max-w-screen-xl" />,
};

type ModalProps = {
	show: boolean;
	size: keyof typeof sizes;
	title: string;
	children?: React.ReactNode;
	onClose: () => void;
};

export const Modal = ({ show, size, title, children, onClose }: ModalProps) => {
	return (
		<Transition appear show={show} as={Fragment}>
			<Dialog as="div" className="fixed inset-0 z-10 overflow-y-auto" onClose={onClose}>
				<div className="min-h-screen px-4 text-center">
					<Transition.Child
						as={Fragment}
						enter="ease-out duration-300"
						enterFrom="opacity-0"
						enterTo="opacity-100"
						leave="ease-in duration-200"
						leaveFrom="opacity-100"
						leaveTo="opacity-0">
						<Dialog.Overlay className="fixed inset-0 bg-black bg-opacity-70" />
					</Transition.Child>

					{/* This element is to trick the browser into centering the modal contents. */}
					<span className="inline-block h-screen align-middle" aria-hidden="true">
						&#8203;
					</span>
					<Transition.Child
						as={Fragment}
						enter="ease-out duration-300"
						enterFrom="opacity-0 scale-95"
						enterTo="opacity-100 scale-100"
						leave="ease-in duration-200"
						leaveFrom="opacity-100 scale-100"
						leaveTo="opacity-0 scale-95">
						{pipeJsx(
							<div className="inline-block p-6 my-8 overflow-hidden text-left align-middle transition-all transform bg-white shadow-xl rounded-md">
								<Dialog.Title as="h3" className="text-lg font-medium leading-6 text-gray-900">
									{title}
								</Dialog.Title>
								{children}
							</div>,
							mergeStyles(sizes[size])
						)}
					</Transition.Child>
				</div>
			</Dialog>
		</Transition>
	);
};
