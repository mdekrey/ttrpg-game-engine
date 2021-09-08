import { getPath } from 'core/hooks/useGameForm';
import { FormState, Path } from 'react-hook-form';

export function ValidationMessages<TFieldValues, TName extends Path<TFieldValues>>({
	errors,
	name,
}: {
	errors: FormState<TFieldValues>['errors'];
	name: TName;
}): JSX.Element | null;
export function ValidationMessages({ message }: { message: string | undefined }): JSX.Element | null;
export function ValidationMessages<TFieldValues, TName extends Path<TFieldValues>>({
	message,
	errors,
	name,
}: {
	message?: string;
	errors?: FormState<TFieldValues>['errors'];
	name?: TName;
}) {
	const resultMessage = message || getPath<any, string>(errors, name!)?.message;
	return resultMessage ? <p className="text-red-dark text-sm">{resultMessage}</p> : null;
}
