import { Modal } from 'src/components/modal/modal';
import { useDisplayedDialogs } from './context';

export function DisplayDialogs() {
	const dialogs = useDisplayedDialogs();

	return (
		<>
			{dialogs.map(({ key, renderer, onCancel, ...props }) => (
				<Modal show key={key} onClose={onCancel ?? (() => {})} {...props}>
					{renderer()}
				</Modal>
			))}
		</>
	);
}
