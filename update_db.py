import pyodbc
conn_str = "Driver={ODBC Driver 17 for SQL Server};Server=HE110749-LP2B;Database=test;UID=sa;PWD=135246Eac;TrustServerCertificate=yes;"
conn = pyodbc.connect(conn_str)
cursor = conn.cursor()

# 1. Update Arabic to Persian (First pass)
cursor.execute("UPDATE Employees SET FirstName = REPLACE(REPLACE(FirstName, N'ي', N'ی'), N'ك', N'ک'), LastName = REPLACE(REPLACE(LastName, N'ي', N'ی'), N'ك', N'ک')")

# 2. Get Hossein Ebrahimi ID
cursor.execute("SELECT Id FROM Employees WHERE PersonnelCode = '110749'")
hossein_row = cursor.fetchone()
if hossein_row:
    hossein_id = hossein_row[0]
    print(f"Hossein ID: {hossein_id}")
    
    # Clear Manager1Id for Fatemeh (100398)
    cursor.execute("UPDATE Employees SET Manager1Id = NULL WHERE PersonnelCode = '100398'")
    
    # Set Manager1Id for Neda (116099) and Soha (127590)
    cursor.execute("UPDATE Employees SET Manager1Id = ? WHERE PersonnelCode IN ('116099', '127590')", (hossein_id,))
    
conn.commit()
print('Database updated successfully.')
