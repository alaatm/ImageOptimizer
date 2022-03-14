* Install latest Azure CLI
* Run the following:
```
az login
az ad sp create-for-rbac -n "image-optimizer" --role contributor --scopes /subscriptions/509ae91c-1e41-451e-84fa-dd82d7696b67/resourceGroups/shiftlee/providers/Microsoft.Web/sites/shiftlee-image-optimizer --sdk-auth
```