import { ClipboardIcon, ClipboardCheckIcon } from '@heroicons/react/outline';
import { ComponentType, ReactNode, useState } from 'react';
import { Sidebar } from 'src/components/sidebar';

export function CopyTextButton({
	toCopy,
	children,
	icon: Icon = ClipboardIcon,
}: {
	toCopy: string;
	children?: ReactNode;
	icon?: ComponentType<JSX.IntrinsicElements['svg']>;
}) {
	const [isCopied, setIsCopied] = useState(false);

	return (
		<Sidebar.Button onClick={copyContents}>
			{!isCopied ? <Icon className="h-5 w-5 pr-1" /> : <ClipboardCheckIcon className="h-5 w-5 pr-1" />} {children}
		</Sidebar.Button>
	);

	async function copyContents() {
		await navigator.clipboard.writeText(toCopy);
		setIsCopied(true);
		setTimeout(() => setIsCopied(false), 1000);
	}
}
