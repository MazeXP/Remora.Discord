#!/bin/bash

# Read versions
readarray -d '' versions < <( \
    find . -maxdepth 1 -type d -regextype posix-extended -regex '\./[0-9]{4}\.[0-9]{1,}' -printf '%f\0' \
  | sort -z -r \
)

# Check if we have to remove old versions
if [[ ${#versions[@]} -gt ${KEEP_VERSIONS} ]]; then
    echo "::group::Removing old versions"
    for (( i=${KEEP_VERSIONS}; i<${#versions[@]}; i++ )); do
        echo "${versions[$i]}"
        echo "rm -rf ${versions[$i]}"
    done
    echo "::endgroup::"
fi

# Generate versions.json
echo "[" > versions.json
echo "  \"main\"," >> versions.json
for (( i=0; i<${KEEP_VERSIONS}; i++ )); do
    if [[ i -eq $((${KEEP_VERSIONS}-1)) ]]; then
        echo "  \"${versions[$i]}\"" >> versions.json
    else
        echo "  \"${versions[$i]}\"," >> versions.json
    fi
done
echo "]" >> versions.json

echo "Generated versions.json"

# Update index.html
sed -i -E "s/url=.*\"/url=.\/${versions[0]}\"/" index.html
echo "Updated index.html"