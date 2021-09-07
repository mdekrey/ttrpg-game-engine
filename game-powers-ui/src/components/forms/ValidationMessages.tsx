export function ValidationMessages({ message }: { message?: string }) {
	return message ? <p className="text-red-dark text-sm">{message}</p> : null;
}
