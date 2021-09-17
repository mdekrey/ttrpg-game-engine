import { createContext, useContext } from 'react';
import operations from 'api/operations';
import { toRxjsApi } from '@principlestudios/openapi-codegen-typescript-rxjs';

const ApiContext = createContext(toRxjsApi(operations, ''));

export const ApiProvider = ApiContext.Provider;

export function useApi() {
	return useContext(ApiContext);
}
