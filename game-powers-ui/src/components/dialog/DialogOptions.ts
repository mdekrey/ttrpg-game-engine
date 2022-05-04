import { ModalProps } from 'components/modal/modal';
import { ReactNode } from 'react';

export type DialogOptions<T = any> = {
	cancellationValue?: T;
	renderer: (onResolve: (result: T) => void) => ReactNode;
} & Pick<ModalProps, 'size' | 'title'>;

export type DialogRenderInfo = {
	key: string;
	onCancel?: () => void;
	renderer: () => ReactNode;
} & Pick<ModalProps, 'size' | 'title'>;
